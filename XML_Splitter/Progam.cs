/// Paul's Branch ////
using System;
using System.Diagnostics;
using System.IO;
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
            DateTime today = DateTime.Today;
            Stopwatch stopWatch = new Stopwatch();

            int index = 0;
            string saveLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\Test_Output";
            string xmlLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\SourceFiles";
            string filePath = "";

            stopWatch.Start();

            foreach (var path in Directory.GetFiles(@xmlLocation))
            {
                String date = GetFileDate(System.IO.Path.GetFileName(path).Substring(20));
                String currentDate = today.ToString("MM/dd/yyyy");

                if (date.Equals(currentDate))    // if the file's date matches the current date proceed to the following
                {
                    //Console.WriteLine("File Found");
                    filePath = xmlLocation + "\\" + System.IO.Path.GetFileName(path);    // instantiate the filePath to equal the xmlLocation and the file being examined

                    XDocument newDoc = XDocument.Load(filePath);
                    
                    XmlReader xmlReader = newDoc.CreateReader();

                    // For AMRDEF Element
                    string amrdefPurpose = "";
                    string amrdefVersion = "";
                    string amrdefCreationTime = "";

                    // For ScheduleExecution Element
                    string scheduleExecutionIrn = "";
                    string scheduleExecutionStarted = "";
                    string scheduleExecutionFinished = "";
                    string scheduleExecutionInitiator = "";

                    while (xmlReader.Read())
                    {
                        if (xmlReader.Name.Equals("AMRDEF")) // If "AMRDEF" Element is found instantiate the following attribute values.
                        {
                            amrdefPurpose = xmlReader.GetAttribute("Purpose");
                            amrdefVersion = xmlReader.GetAttribute("version");
                            amrdefCreationTime = xmlReader.GetAttribute("CreationTime");
                        }

                        if (xmlReader.Name.Equals("ScheduleExecution")) // If "ScheduleExecution" Element is found instantiate the following attribute values.
                        {
                            scheduleExecutionIrn = xmlReader.GetAttribute("Irn");
                            scheduleExecutionStarted = xmlReader.GetAttribute("started");
                            scheduleExecutionFinished = xmlReader.GetAttribute("finished");
                            scheduleExecutionInitiator = xmlReader.GetAttribute("Initiator");
                            break;
                        }
                    }

                    var query = newDoc.Root.Descendants("ScheduleExecution").DescendantNodes(); // Get all nodes found within, but not including, the "SceduleExecution" Element
                    XElement header = new XElement("ScheduleExecution", new XAttribute("Irn", scheduleExecutionIrn), new XAttribute("started", scheduleExecutionStarted), new XAttribute("finished", scheduleExecutionFinished), new XAttribute("Initiator", scheduleExecutionInitiator), query);  // Take the Header ("ScheduleExecution") of the current XML File being examined

                    newDoc.Descendants("ScheduleExecution").Remove(); // Remove the Header ("ScheduleExecution") from the newDoc object

                    int elementsPerFile = GetNumberOfElementsPerNewFile(newDoc);

                    // adds {elementsPerFile} elements to a new file that then saves to a folder
                    foreach (var batch in newDoc.Root.Elements().InSetsOf(elementsPerFile))
                    {
                        var finalDoc = new XDocument(
                             new XElement("AMRDEF", new XAttribute("Purpose", amrdefPurpose), new XAttribute("version", amrdefVersion), new XAttribute("CreationTime", amrdefCreationTime), header, batch));

                        finalDoc.Save($"{saveLocation}\\meterDataFile_{++index}.xml");
                    }
                }

            }

            stopWatch.Start();

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
             * Elements each new file showed display */
            static int GetNumberOfElementsPerNewFile(XDocument newDoc)
            {
                double scalarVariableCount = newDoc.Root.Elements().Count();
                double numberOfElementsPerFile = Math.Ceiling(scalarVariableCount / 24);
                int elementAmount = Convert.ToInt32(numberOfElementsPerFile);

                return elementAmount;
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
}