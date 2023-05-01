/****** Jim's Branch ***********/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;

namespace XML_Splitter
{
    public class Progam
    {
        static void Main(string[] args)
        {
            Amrdef amrdef = new Amrdef();
            ScheduleExecution scheduleExecution = new ScheduleExecution();
            String currentDate = DateTime.Today.ToString("MM/dd/yyyy");
            
            String date;

            // These Paths are used for testing purposes
            //string saveLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\Test_Output";
            //string xmlLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\SourceFiles";

            string saveLocation = ".\\DailyMDMDataOutput"; 
            string xmlLocation = "\\\\automate2101\\D\\Applications\\AMRDailyReadProcessing\\FullIntDataSource";

            string filePath;

            // Examine each file within the given path
            foreach (var path in Directory.GetFiles(@xmlLocation))
            {
                date = GetFileDate(System.IO.Path.GetFileName(path).Substring(20));

                if (date.Equals(currentDate))    // if the file's date matches the current date proceed to the following
                {
                    filePath = xmlLocation + "\\" + System.IO.Path.GetFileName(path);    // instantiate the filePath to equal the xmlLocation and the file being examined

                    XDocument newDoc = XDocument.Load(filePath);
                    XmlReader xmlReader = newDoc.CreateReader();

                    GetElementAttributes(xmlReader, amrdef, scheduleExecution); // Get attributes of AMRDEF and ScheduleExecution elements being examined

                    var query = newDoc.Root.Descendants("ScheduleExecution").DescendantNodes(); // Get all nodes found within, but not including, the "SceduleExecution" Element

                    // Create "ScheduleExecution" element with proper attributes
                    XElement scheduleExecutionElement = new XElement("ScheduleExecution", new XAttribute("Irn", scheduleExecution.Irn), new XAttribute("started", scheduleExecution.Started), new XAttribute("finished", scheduleExecution.Finished), new XAttribute("Initiator", scheduleExecution.Initiator), query);

                    newDoc.Descendants("ScheduleExecution").Remove(); // Remove the Header ("ScheduleExecution") from the newDoc object

                    // Parse large xml file and create new smaller files
                    CreateFiles(newDoc, amrdef, scheduleExecutionElement, saveLocation);
                }

            }

            /************************************************************************************************/
            /*The Following Assigns the necessary attributes to the AMRDEF and ScheduleExecution elements ***/
            static void GetElementAttributes(XmlReader xmlReader, Amrdef amrdef, ScheduleExecution scheduleExecution)
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.Name.Equals("AMRDEF")) // If "AMRDEF" Element is found instantiate the following attribute values.
                    {
                        amrdef.Purpose = xmlReader.GetAttribute("Purpose");
                        amrdef.Version = xmlReader.GetAttribute("version");
                        amrdef.CreationTime = xmlReader.GetAttribute("CreationTime");
                    }

                    if (xmlReader.Name.Equals("ScheduleExecution")) // If "ScheduleExecution" Element is found instantiate the following attribute values.
                    {
                        scheduleExecution.Irn = xmlReader.GetAttribute("Irn");
                        scheduleExecution.Started = xmlReader.GetAttribute("started");
                        scheduleExecution.Finished = xmlReader.GetAttribute("finished");
                        scheduleExecution.Initiator = xmlReader.GetAttribute("Initiator");
                        break;
                    }
                }
            }

            /****************************************************************************/
            /*The Following modifies the file's date in the format MM/dd/yyy*/
            static string GetFileDate(string fileDate)
            {
                String date = fileDate;
                date = date.Replace('_', '/');
                date = date.Substring(0, 10);
                return date;
            }

            /***************************************************************************/
            /* Below Counts the Original XML file's Elements which then determines, and returns, how many
             * Elements each new file should display */
            static int GetNumberOfElementsPerNewFile(XDocument newDoc)
            {
                double scalarVariableCount = newDoc.Root.Elements().Count();
                double numberOfElementsPerFile = Math.Ceiling(scalarVariableCount / 24);
                int elementAmount = Convert.ToInt32(numberOfElementsPerFile);

                return elementAmount;
            }

            /************************************************************************************************/
            /* Uses GetNumberOfElementsPerFile() function to calculate how many lines are to be added to each file. The 
             * function then begins creating the smaller XML files */ 

            static void CreateFiles(XDocument newDoc, Amrdef amrdef, XElement scheduleExecutionElement, string saveLocation)
            {
                int elementsPerFile = GetNumberOfElementsPerNewFile(newDoc);
                int index = 0;

                // adds {elementsPerFile} elements to a new file that then saves to a folder
                foreach (var batch in newDoc.Root.Elements().InSetsOf(elementsPerFile))
                {
                    var finalDoc = new XDocument(
                         new XElement("AMRDEF", new XAttribute("Purpose", amrdef.Purpose), new XAttribute("version", amrdef.Version), new XAttribute("CreationTime", amrdef.CreationTime), scheduleExecutionElement, batch));

                    finalDoc.Save($"{saveLocation}\\meterDataFile_{++index}.xml");
                }
            }
        }
    }

    /*********************************************************************************************/
    /**** This class assists in determing how many elements should be included in one "batch" ****/
    public static class IEnumerableExtensions
    {
        public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
        {
            List<T> toReturn = new List<T>(max);
            foreach (var item in source)
            {
                toReturn.Add(item);
                if (toReturn.Count == max)
                {
                    yield return toReturn;
                    toReturn = new List<T>(max);
                }
            }
            if (toReturn.Any())
            {
                yield return toReturn;
            }
        }
    }

    /************************************************************************************************/
    /**** This class assists in determing the AMRDEF attributes when creating a new element *********/
    class Amrdef
    {
        public string Purpose { get; set; }
        public string Version { get; set; }
        public string CreationTime { get; set; }
    }

    /************************************************************************************************/
    /**** This class assists in determing ScheduleExecution attributes when creating a new element **/
    class ScheduleExecution
    {
        public string Irn { get; set; }
        public string Started { get; set; }
        public string Finished { get; set; }
        public string Initiator { get; set; }
    }
}
