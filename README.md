# Virus Scanner
Clam AV virus scanning tool

##Getting started 
Install desktop docker (https://docs.docker.com/docker-for-windows/install/) 
Run 
`docker run -d -p 3310:3310 mkodockx/docker-clamav:alpine`
 
This will download and run a docker image that will run on port 3310
Open the code in visual studio and press F5 
A console window will appear. Copy a file into the "Drop" folder.  The location of the drop folder will be specified in the appSettings file.
The console window will print out the progress of the file. 
If it has been successful it will be moved into the "Clean" folder if not it will be moved into the "Quarantine" folder.


