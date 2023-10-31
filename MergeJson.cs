using System.IO;
using Functions.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.Functions
{
    public static class MergeJson
    {
        [FunctionName(nameof(MergeJson))]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<JToken>(requestBody);

            var left = data.SelectTokenCaseInsensitive("left");
            var right = data.SelectTokenCaseInsensitive("right");

            var merged = left.MergeWith(right);
            if (merged == null)
            {
                return new OkObjectResult("");
            }

            log.LogInformation("JSON objects merged successfully.");

            return new OkObjectResult(merged);
        }
    }
}