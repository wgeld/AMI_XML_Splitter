using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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
            string saveLocation = "\\\\Automate2101\\d\\Applications\\MDMIntervalDataParser\\DailyMDMDataOutput";
            string xmlLocation = "\\\\Automate2101\\d\\Applications\\AMRDailyReadProcessing";
            //string saveLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\Test_Output";
            //string xmlLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\SourceFiles";

            string filePath = "";

            stopWatch.Start();

            foreach (var path in Directory.GetFiles(@xmlLocation))
            {
                String aaa = System.IO.Path.GetFileName(path);
                Console.WriteLine(aaa);

                if(!aaa.Equals("AAA_SolarAnalysis.txt"))
                {
                    String electric = System.IO.Path.GetFileName(path).Substring(17);
                    electric = electric.Substring(0, 8);
                    Console.WriteLine(electric);

                    if (!electric.Equals("ELECTRIC"))
                    {
                        String date = GetFileDate(System.IO.Path.GetFileName(path).Substring(20));
                        String currentDate = today.ToString("MM/dd/yyyy");
                        String fileNameDate = today.ToString("MM-dd-yyyy");

                        if (date.Equals(currentDate))    // if the file's date matches the current date proceed to the following
                        {
                            //Console.WriteLine("File Found");
                            filePath = xmlLocation + "\\" + System.IO.Path.GetFileName(path);    // instantiate the filePath to equal the xmlLocation and the file being examined

                            XDocument newDoc = XDocument.Load(filePath);

                            var query = newDoc.Descendants("ScheduleExecution");
                            XElement header = new XElement("ScheduleExecution", query);  // Take the Header ("ScheduleExecution") of the current XML File being examined

                            newDoc.Descendants("ScheduleExecution").Remove(); // Remove the Header ("ScheduleExecution")

                            int elementAmount = GetNumberOfElementsPerNewFile(newDoc);

                            // adds {elementsPerFile} elements to a new file that then saves to a folder
                            foreach (var batch in newDoc.Root.Elements().InSetsOf(elementAmount))
                            {
                                var finalDoc = new XDocument(
                                     new XElement("AMRDEF", header, batch));

                                finalDoc.Save($"{saveLocation}\\meterDataFile_{fileNameDate}_{++index}.xml");
                            }
                        }
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