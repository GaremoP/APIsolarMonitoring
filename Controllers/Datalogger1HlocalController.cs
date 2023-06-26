using APIsolarMonitoring.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace APIsolarMonitoring.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Datalogger1HlocalController : ControllerBase
    {
        private const string CsvFilePath = FilesPaths.Datalogger1hLocal24;

        [HttpGet]
        public ActionResult<IEnumerable<dynamic>> Get([FromQuery] Datalogger1Filters filters)
        {
            var data = ReadCsvFile(filters);
            var jsonData = JsonConvert.SerializeObject(data);
            var filteredJson = FilterJsonData(jsonData, filters);
            return Ok(filteredJson);
        }

        [HttpGet("Graph")]
        public ActionResult<IEnumerable<dynamic>> Graph([FromQuery] Datalogger1Filters filters)
        {
            var data = ReadCsvFile(filters);
            var jsonData = JsonConvert.SerializeObject(data);
            return Ok(jsonData);
        }
        private List<dynamic> ReadCsvFile(Datalogger1Filters filters)
        {
            var data = new List<dynamic>();

            using (var reader = new StreamReader(CsvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>();

                    if (IsWithinDateRange(record.Datetime, filters.StartDate, filters.EndDate))
                    {
                        data.Add(record);
                    }
                }
            }

            return data;
        }


        private static bool IsWithinDateRange(string recordTime, DateTime? startDate, DateTime? endDate)
        {
            DateTime.TryParseExact(recordTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeRecord);

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
                var filteredObject = new JObject();
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
