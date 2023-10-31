using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Canonical.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Canonical.Functions
{
    public class GenerateStateItemHash
    {
        [FunctionName(nameof(GenerateStateItemHash))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            ILogger log)
        {
            try
            {
                var reqBody = await req.Content.ReadAsStringAsync();
                var reqBodyObject = (JObject)JsonConvert.DeserializeObject(reqBody);
                var eventType = reqBodyObject["eventType"].ToString();
                if (eventType != "Youforce.OrganizationalUnit.Single")
                {
                    throw new UnexpectedEventException(eventType);
                }
                var eventClass = JsonConvert.DeserializeObject<YouForceOrganizationalUnit>(reqBodyObject["eventBody"].ToString());
                var objectHash = ComputeSha256Hash(eventClass.ToString());
                return new OkObjectResult(new { sha256sum = objectHash });
            }
            catch (UnexpectedEventException ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (NullReferenceException)
            {
                log.LogError("One or more required properties are not provided.");
                return new BadRequestObjectResult("One or more required properties are not provided.");
            }
            catch (JsonException ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError("Unexpected error happened: " + ex.Message);
                return new ObjectResult("Unexpected error happened: " + ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        private static string ComputeSha256Hash(string input)
        {
            using (var sha256Hash = SHA256.Create())
            {
                var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < data.Length; i++)
                {
                    stringBuilder.Append(data[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }
}
