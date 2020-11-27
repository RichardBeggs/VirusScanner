using Microsoft.Extensions.Configuration;
using nClam;
using System;
using System.IO;
using System.Linq;

namespace VirusScannerConsole
{
    internal class Program
    {
        private const string ClamAvServer = "ClamAVServer";
        private const string FolderLocations = "FolderLocations";
        private static IConfiguration _config;

        private static string GetTargetLocation(TargetLocations targetLocation)
        {
            //Get Folder locations from the appsettings
            var fileLocations = new Settings.FolderLocations();

            //bind the config settings to an object
            _config.GetSection(FolderLocations).Bind(fileLocations);

            switch (targetLocation)
            {
                case TargetLocations.Clean:
                    return fileLocations.CleanFolder;

                case TargetLocations.Quarantine:
                    return fileLocations.QuarantineFolder;

                default:
                    return fileLocations.ErrorFolder;
            }
        }

        private static void Main(string[] args)
        {
            //read the appsettings per environment
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            _config = builder.Build();

            //Set the folder that will be monitored.  This is set in the appsettings per environment
            SetUpFolderMonitoring();
        }

        private static void Monitor(string path)
        {
            Directory.CreateDirectory(path);

            // Create a new FileSystemWatcher and set its properties.
            using var watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite
                               | NotifyFilters.CreationTime
                               | NotifyFilters.LastAccess
                               | NotifyFilters.FileName
            };

            // Watch for changes in LastWrite times
            // Add event handlers.
            watcher.Created += OnChanged;
            watcher.Error += OnError;

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            Console.WriteLine("Press 'q' to quit the sample.");
            while (Console.Read() != 'q')
            {
            }
        }

        private static void MoveToFolder(string sourceFile, TargetLocations targetLocation)
        {
            var targetLocationPath = GetTargetLocation(targetLocation);
            var destFile = Path.Combine(targetLocationPath, Path.GetFileName(sourceFile));

            // To copy a folder's contents to a new location:
            // Create a new target folder.
            // If the directory already exists, this method does not create a new directory.
            Directory.CreateDirectory(targetLocationPath);

            // To move a file to another location and
            // overwrite the destination file if it already exists.
            Console.WriteLine($"Moving file to { targetLocation } location");

            File.Move(sourceFile, destFile, true);
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            Scan(e.FullPath);
        }

        //  This method is called when the FileSystemWatcher detects an error.
        private static void OnError(object source, ErrorEventArgs e)
        {
            //  Show that an error has been detected.
            Console.WriteLine("The FileSystemWatcher has detected an error");
            //  Give more information if the error is due to an internal buffer overflow.
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                Console.WriteLine(("The file system watcher experienced an internal buffer overflow: " +
                                   e.GetException().Message));
            }
        }

        private static async void Scan(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            try
            {
                ClamScanResult scanResult = null;
                await using (var f = File.OpenRead(filepath))
                {
                    Console.WriteLine("ClamAV scan begin for file {0}", f.Name);
                    var clam = SetupClamClient();
                    scanResult = await clam.SendAndScanFileAsync(f);
                }

                switch (scanResult.Result)
                {
                    case ClamScanResults.Clean:
                        Console.WriteLine($"The file is clean! ScanResult:{scanResult.RawResult}");
                        MoveToFolder(filepath, TargetLocations.Clean);
                        break;

                    case ClamScanResults.VirusDetected:
                        Console.WriteLine(
                            $"Virus Found! Virus name: {scanResult.InfectedFiles.FirstOrDefault().VirusName}");
                        MoveToFolder(filepath, TargetLocations.Quarantine);
                        break;

                    case ClamScanResults.Error:
                        Console.WriteLine(
                            $"An error occured while scanning the file! ScanResult: {scanResult.RawResult}");
                        break;

                    case ClamScanResults.Unknown:
                        Console.WriteLine(
                            $"Unknown scan result while scanning the file! ScanResult: {scanResult.RawResult}");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ClamAV Scan Exception: {0}", ex.ToString());
            }

            Console.WriteLine("ClamAV scan completed for file {0}", filename);
        }

        private static ClamClient SetupClamClient()
        {
            var settings = new Settings.ClamAvServer();
            _config.GetSection(ClamAvServer).Bind(settings);

            var port = Convert.ToInt32(settings.Port);
            var url = settings.Url;
            return new ClamClient(url, port);
        }

        /// <summary>
        /// Set up the folder to monitor
        /// </summary>
        private static void SetUpFolderMonitoring()
        {
            var folderLocations = new Settings.FolderLocations();
            _config.GetSection(FolderLocations).Bind(folderLocations);

            var folderPath = folderLocations.DropFolder;
            Monitor(folderPath);
        }
    }
}