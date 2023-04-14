using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

DateTime today = DateTime.Today;
Stopwatch stopWatch = new Stopwatch();

int index = 0;

string saveLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\Test_Output";
string xmlLocation = "Z:\\IT_Development\\Projects\\Active\\MDMIntervalDataParcer\\SourceFiles";
string filePath = "";

stopWatch.Start();

foreach (var path in Directory.GetFiles(@xmlLocation))
{
    //The Following Parses and modifies the file's date
    String date = System.IO.Path.GetFileName(path).Substring(20);
    date = date.Replace('_', '/');
    date = date.Substring(0, 10);

    //String currentDate = today.ToString("MM/dd/yyyy");

    if (date.Equals("04/07/2023"))    // if the files date matches the current date proceed to the following
    {
        Console.WriteLine("Found date");
        filePath = xmlLocation + "\\" + System.IO.Path.GetFileName(path);    // instantiate the filePath to equal the xmlLocation and the file being examined

        // add all elements of the XML file to a newly created XDocument called 'newDoc'
        // after adding all element trim unwanted information
        XDocument newDoc = XDocument.Load(filePath);
        var query = newDoc.Descendants("ScheduleExecution");

        XElement header = new XElement("ScheduleExecution", query);

        newDoc.Descendants("ScheduleExecution").Remove();

        
        /******* JUST INCASE THE USERS WANT TO REMOVE SPECIFIED ELEMENTS *****************/
        //newDoc.Descendants("MetersNotRead").Remove();
        //newDoc.Descendants("MetersRead").Remove();
        //newDoc.Descendants("ScheduleExecution").Remove();
        /*********************************************************************************/

        /******************  OLD CODE **********************************/
        //foreach (var element in XDocument.Load(filePath).Elements())
        //{
        //    newDoc.Add(element);
        //}

        /********** Below is code that attempted to count all elements within the file...could possibly be used again at some point ********/
        //counts number of elements in XML file, limits the size(number of elements) of the new split file to make it so it fits in 24 files.
        //foreach (var element in newDoc.Root.Elements())
        //{
        //    elementCount++;
        //}
        //elementsPerFile = (elementCount / 24) + 30;
        /***************************************************************/

        double scalarVariableCount = newDoc.Root.Elements().Count();
        Console.WriteLine("Scalar Variable Count: " + scalarVariableCount);
        double numberOfElementsPerFile = Math.Ceiling(scalarVariableCount / 24);
        Console.WriteLine("Number of Elements per New File: " + numberOfElementsPerFile);
        int elementAmount = Convert.ToInt32(numberOfElementsPerFile);

        // adds {elementsPerFile} elements to a new file that then saves to a folder
        foreach (var batch in newDoc.Root.Elements().InSetsOf(elementAmount))
        {
            var finalDoc = new XDocument(
                 new XElement("AMRDEF", header, batch));

            finalDoc.Save($"{saveLocation}\\meterDataFile_{++index}.xml");
        }

    }
}

Console.WriteLine("DONE!");
stopWatch.Stop();

Console.WriteLine($"Execution Time: {stopWatch.ElapsedMilliseconds} ms");

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
