# AMI_xml_Splitter

## About 

The main function of this Project is to accept a large XML file and parse that file into smaller, more manageable files. The number of files created will not exceed 24 and each file will be parsed equally.

Currently, the system accepts XML files relating to gas meter readings. This process is subject to change.

## Process

 - The system examines each file from the desired path
 - If a file matches that of the current date the system will extract the necessary "Header" information and store the contents within the "Header" accordingly. The "Header" attribute values are to also be stored as variables
 - The "Header" and its contents are removed from the XML file
 - The system counts the number of Elements remaining in the XML file and makes the necessary calculations to determine how many lines are applied in each new file. This is to ensure the number of files being created does not exceed 24.
 - The system creates the new smaller XML files. This is done by attaching all the stored "Header" information to a batch of Elements, previously calculated. 
 
 ## Diagram 
 ![image](https://user-images.githubusercontent.com/109157319/235451377-e73ff8cd-7132-41ad-91f5-c6ba59b04ce4.png)

 ## Technologies applied
 
 - C#
 - AutoMate
