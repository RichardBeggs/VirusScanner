namespace VirusScannerConsole
{
    public class Settings
    {
        public class ClamAvServer
        {
            public string Port { get; set; }
            public string Url { get; set; }
        }

        public class FolderLocations
        {
            public string CleanFolder { get; set; }
            public string DropFolder { get; set; }
            public string ErrorFolder { get; set; }
            public string QuarantineFolder { get; set; }
            public string UnknownFolder { get; set; }
        }
    }
}