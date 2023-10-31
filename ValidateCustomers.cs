using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Salesforce.Functions.Models;
using Salesforce.Functions.Validators;

namespace Salesforce.Functions
{
    public static class ValidateCustomers
    {
        [FunctionName(nameof(ValidateCustomers))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                var requestBody = new StreamReader(req.Body).ReadToEnd();
                var input = JsonConvert.DeserializeObject<List<Customer>>(requestBody);
                var result = new CustomerValidationResult();

                var validator = new CustomerValidator();
                result = CustomerValidator.ValidateCustomers(input);
                return new OkObjectResult(result);
            }
            catch (JsonException jsonEx)
            {
                log.LogError($"Json deserialization failed. {jsonEx.Message}");
                return new BadRequestObjectResult($"Json deserialization failed. {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                log.LogError($"Unknown error occured. {ex.Message}");
                return new BadRequestObjectResult($"Unknown error occured. {ex.Message}");
            }
        }
    }
}
