using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlusPort.LMS.Functions.Models;
using PlusPort.LMS.Functions.Validators;

namespace PlusPort.LMS.Functions
{
    public static class UpsertPlusPortDebtorAndDepartment
    {
        private static readonly string companyId = Environment.GetEnvironmentVariable("PLUSPORT_LMS_COMPANYID");
        private static readonly string companyGuid = Environment.GetEnvironmentVariable("PLUSPORT_LMS_COMPANYGUID");
        private static readonly string serviceEndpoint = Environment.GetEnvironmentVariable("PLUSPORT_LMS_ENDPOINT");

        [FunctionName(nameof(UpsertPlusPortDebtorAndDepartment))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var GROUP_CODE = "Visma_raet";
                var PARENT_CODE = "Visma_Raet_Customers";
                var validator = new SalesforceCustomerValidator();

                var requestBody = new StreamReader(req.Body).ReadToEnd();

                var salesforceCustomer = JsonConvert.DeserializeObject<SalesforceCustomer>(requestBody);

                var validationResults = validator.Validate(salesforceCustomer);
                if (!validationResults.IsValid)
                {
                    return new BadRequestErrorMessageResult(validationResults.ToString());
                }

                if (!salesforceCustomer.Active || salesforceCustomer.Status.IsOneOf("Bankrupt", "Former Customer", "Former Customer (Merger)"))
                {
                    GROUP_CODE = "Visma_Raet_Inactive";
                    PARENT_CODE = "Visma_Raet_Customers_Inactive";
                }

                log.LogInformation("Processing {customerId} {customerName}.", salesforceCustomer.CustomerId, salesforceCustomer.CustomerName);

                var client = new AcademyClient(AcademyClient.EndpointConfiguration.BasicHttpBinding_IAcademy, serviceEndpoint);

                var debtorHash = HashWithSHA256(companyId + companyGuid + salesforceCustomer.CustomerId);

                var debtor = new Debtor
                {
                    Code = salesforceCustomer.CustomerId,
                    Name = salesforceCustomer.CustomerName,
                    GroupCode = GROUP_CODE,
                    isActive = salesforceCustomer.Active && !salesforceCustomer.Status.IsOneOf("Bankrupt", "Former Customer", "Former Customer (Merger)")
                };

                var debtorResult = await client.setDebtorAsync(int.Parse(companyId), debtorHash, debtor);

                if (debtorResult == "OK")
                {
                    var departmentHash = HashWithSHA256(companyId + companyGuid);

                    var department = new Department
                    {
                        Code = salesforceCustomer.CustomerId,
                        DebtorCode = PARENT_CODE == "Visma_Raet_Customers" ? salesforceCustomer.CustomerId : null,
                        Name = salesforceCustomer.CustomerName,
                        ParentCode = PARENT_CODE
                    };
                    var departments = new Department[] { department };

                    var departmentResult = await client.setDepartmentListAsync(int.Parse(companyId), departmentHash, departments);

                    if (departmentResult == "OK")
                    {
                        log.LogInformation("Debtor and department upserted. {customerId} {customerName}", debtor.Code, debtor.Name);
                        return new OkResult();
                    }

                    log.LogError("Department upsert failed. {customerId} {customerName}", department.Code, department.Name);

                    return new BadRequestErrorMessageResult(departmentResult);
                }

                log.LogError("Debtor upsert failed. {customerId} {customerName}", debtor.Code, debtor.Name);

                return new BadRequestErrorMessageResult(debtorResult);
            }
            catch (JsonException jsEx)
            {
                return new BadRequestErrorMessageResult(jsEx.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unexpected error occured. " + ex.Message);
                throw;
            }
        }
        private static string HashWithSHA256(string value)
        {
            var byteArray = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(byteArray);
        }

        public static bool IsOneOf(this object item, params object[] options) => options.Contains(item);
    }
}
