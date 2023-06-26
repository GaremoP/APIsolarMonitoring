using Microsoft.AspNetCore.Mvc;
using APIsolarMonitoring.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using CsvHelper;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace APIsolarMonitoring.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InverterController : ControllerBase
    {
        private const string CsvFilePath = FilesPaths.InverterString1;

        [HttpGet]
        public ActionResult<IEnumerable<dynamic>> Get([FromQuery] InverterFilter filters)
        {
            var data = ReadCsvFile(filters);
            var jsonData = JsonConvert.SerializeObject(data);
            var filteredJson = FilterJsonData(jsonData, filters);
            return Ok(filteredJson);
        }
        [HttpGet("Graph")]
        public ActionResult<IEnumerable<dynamic>> Graph([FromQuery] InverterFilter filters)
        {
            var data = ReadCsvFile(filters);
            var jsonData = JsonConvert.SerializeObject(data);
            return Ok(jsonData);
        }


        private List<dynamic> ReadCsvFile(InverterFilter filters)
        {
            using (var reader = new StreamReader(CsvFilePath))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                var data = new List<dynamic>();
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    if (IsWithinDateRange(record.Time, filters.StartDate, filters.EndDate))
                    {
                        data.Add(record);
                    }
                }
                return data;
            }
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

        private static string FilterJsonData(string jsonData, InverterFilter filters)
        {
            JArray originalArray = JArray.Parse(jsonData);
            JArray filteredArray = new JArray();

            // Get names of properties and replace dots with underscores to match real keys
            var listOfProperties = filters.GetType().GetProperties().Skip(2).ToList();//Skip start and finish date
            var filterNameMatch = listOfProperties.Select(p =>p.Name).ToList();


            foreach (JObject instance in originalArray.Cast<JObject>())
            {
                //Each instance of the JSON
                JObject filteredObject = new JObject();
                filteredObject["Time"] = instance["Time"];

                foreach (var keyvaluepair in instance)
                {
                    var key = keyvaluepair.Key.Replace(".", "_").Replace(" ", "_").Replace("(", "").Replace(")", "");

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

