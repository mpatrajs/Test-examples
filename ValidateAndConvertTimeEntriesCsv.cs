using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Capman.Functions.Models;
using Capman.Functions.Validators;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Capman.Functions
{
    public static class ValidateAndConvertTimeEntriesCsv
    {
        [FunctionName(nameof(ValidateAndConvertTimeEntriesCsv))]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            TimeEntryValidationResult result;

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                Delimiter = ";"
            };

            using (var reader = new StringReader(RemoveQuotes(req)))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                try
                {
                    var timeEntries = csv.GetRecordsAsync<CanonicalTimeEntry>();

                    var validator = new TimeEntryValidator();
                    result = await TimeEntryValidator.ValidateTimeEntriesAsync(timeEntries);
                }
                catch (TypeConverterException csvEx)
                {
                    log.LogError($"Failed to deserialize CSV. {csvEx.Message}");
                    return new BadRequestObjectResult($"Failed to deserialize CSV. {csvEx.Message}");
                }
                catch (Exception ex)
                {
                    log.LogError($"Unexpected error occured. {ex.Message}");
                    return new BadRequestObjectResult($"Unexpected error occured. {ex.Message}");
                }
            }

            return new OkObjectResult(result);
        }

        private static string RemoveQuotes(HttpRequest req)
        {
            var noQuotes = new StreamReader(req.Body).ReadToEnd();
            return noQuotes.Replace("\"", "");
        }
    }
}
