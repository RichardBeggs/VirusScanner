# Virus Scanner
Clam AV virus scanning tool

## Introduction 
The idea is to run a virus scan on a file as so as it appears in a folder.  To do this I have leveraged a docker image from docker image location https://hub.docker.com/r/mkodockx/docker-clamav/ that runs allows you to call make a rest call to the service.  I have also used the nuget nClam https://github.com/tekmaven/nClam which has made the interation with .net core very straight forward.  The project works to demonstrate the concept running locally but there would need to be significant changes to scale out.  The FileWatcher will not work well on remote folder locations such as AWS or Azure so the virus scan would need to be triggered by the file upload.  These changes would depend on the enviroment that is using used to store the files and the file upload process.   

## Getting started 
Install desktop docker (https://docs.docker.com/docker-for-windows/install/) 

Run `docker run -d -p 3310:3310 mkodockx/docker-clamav:alpine` 
 
This will download and run a docker image that will run on port 3310
Open the code in visual studio and press F5 
A console window will appear. Copy a file into the "Drop" folder.  The location of the drop folder will be specified in the appSettings file.
The console window will print out the progress of the file. 
If it has been successful it will be moved into the "Clean" folder if not it will be moved into the "Quarantine" folder.


