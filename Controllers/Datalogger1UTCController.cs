using APIsolarMonitoring.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace APIsolarMonitoring.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Datalogger1UTCController : ControllerBase
    {
        private const string CsvFilePath = FilesPaths.Datalogger1;

        [HttpGet]
        public ActionResult<IEnumerable<dynamic>> Get([FromQuery] Datalogger1Filters filters)
        {
            var data = ReadCsvFile(filters);
            var jsonData = JsonConvert.SerializeObject(data);
            var filteredJson = FilterJsonData(jsonData, filters);
            return Ok(filteredJson);
        }

        private List<dynamic> ReadCsvFile(Datalogger1Filters filters)
        {
            using var reader = new StreamReader(CsvFilePath);
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
            var data = new List<dynamic>();
            var records = csv.GetRecords<dynamic>();
            foreach (var record in records)
            {
                if (IsWithinDateRange(record.Datetime, filters.StartDate, filters.EndDate))
                {
                    data.Add(record);
                }
            }
            return data;
        }

        private static bool IsWithinDateRange(string recordTime, DateTime? startDate, DateTime? endDate)
        {
            var timeRecord = DateTime.Parse(recordTime);

            if (startDate.HasValue && timeRecord < startDate.Value)
            {
                return false;
            }

            if (endDate.HasValue && timeRecord > endDate.Value)
            {
                return false;
            }

            return true;
        }
        private static string FilterJsonData(string originalJson, Datalogger1Filters filters)
        {
            JArray originalArray = JArray.Parse(originalJson);
            JArray filteredArray = new JArray();

            // Get names of properties and replace dots with underscores to match real keys
            var listOfProperties = filters.GetType().GetProperties().Skip(2).ToList();
            var filterNameMatch = listOfProperties.Select(p => p.Name).ToList();

            foreach (JObject instance in originalArray.Cast<JObject>())
            {
                //Each instance of the JSON
                JObject filteredObject = new JObject();
                filteredObject["Datetime"] = instance["Datetime"];

                foreach (var keyvaluepair in instance)
                {
                    var key = keyvaluepair.Key.Replace(".", "_");

                    if (filterNameMatch.Contains(key))
                    {
                        var filterProperty = listOfProperties[filterNameMatch.IndexOf(key)];
                        if ((bool)filterProperty.GetValue(filters))
                        {
                            filteredObject[keyvaluepair.Key] = instance[keyvaluepair.Key];
                        }
                    }
                } 

                filteredArray.Add(filteredObject);
            }

            return filteredArray.ToString(Formatting.None);
        }

    }
}
