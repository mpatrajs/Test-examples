using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Netive.Functions.Helpers;
using Netive.Functions.Models;

namespace Netive.Functions
{
    public class ValidateTimeEntries
    {
        [FunctionName("ValidateTimeEntries")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var doc = new XmlDocument();
                doc.LoadXml(requestBody);
                log.LogInformation("Serializing XML entries");
                var canoncalInterimHourList = SerializeInterimHours(doc);
                log.LogInformation("Getting latest revisions for each day");
                var latestRevisions = GroupHoursByDateAndType(canoncalInterimHourList);
                log.LogInformation("Validating latest revision");
                var validatedHours = ValidateHours(latestRevisions);
                return new OkObjectResult(validatedHours);
            }
            catch (Exception ex) when (ex is NullReferenceException || ex is XmlException)
            {
                log.LogError($"Invalid XML provided: {ex.Message}");
                return new BadRequestObjectResult($"Invalid XML provided: {ex.Message}");
            }
            catch (Exception ex)
            {
                log.LogError($"Unexpected error happened: {ex.Message}");
                return new ObjectResult($"Unexpected error happened: {ex.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        private static List<CanonicalInterimHours> SerializeInterimHours(XmlDocument interimHoursXml)
        {
            var timeEntries = interimHoursXml.GetElementsByTagName("TimeInterval");
            var canoncalInterimHourList = new List<CanonicalInterimHours>();
            foreach (XmlNode timeEntry in timeEntries)
            {
                var isBillable = false;
                var rateType = timeEntry["Id"].SelectSingleNode("*[@name='rateid']");
                if (rateType?.InnerText == "RTNCB")
                {
                    continue;
                }
                for (var i = 0; i < timeEntry.Attributes.Count; i++)
                {
                    if (timeEntry.Attributes[i].Name == "billable")
                    {
                        if (timeEntry.Attributes[i].Value == "true")
                        {
                            isBillable = true;
                        }
                    }
                }

                if (isBillable)
                {
                    var interimHourEntry = ComposeInterimHourEntry(interimHoursXml, timeEntry, rateType);
                    canoncalInterimHourList.Add(interimHourEntry);
                }
            }
            return canoncalInterimHourList;
        }
        private static CanonicalInterimHours ComposeInterimHourEntry(XmlDocument interimHoursXml, XmlNode timeEntry, XmlNode rateType)
        {
            var accountNumber = interimHoursXml["TimeCard"]["AdditionalData"]["StaffingAdditionalData"]["CustomerReportingRequirements"]["DepartmentCode"]?.InnerText;
            var jobCode = interimHoursXml["TimeCard"]["AdditionalData"]["StaffingAdditionalData"]["CustomerReportingRequirements"]["CustomerJobCode"]?.InnerText;
            var consultantName = interimHoursXml["TimeCard"]["ReportedResource"]["Person"]["PersonName"]["FormattedName"]?.InnerText;
            var customerJobDescription = interimHoursXml["TimeCard"]["AdditionalData"]["StaffingAdditionalData"]["CustomerReportingRequirements"]["CustomerJobDescription"]?.InnerText;

            var revisionNumber = timeEntry["Id"].SelectSingleNode("*[@name='revision']");
            var duration = timeEntry["Duration"]?.InnerText;
            var rateOrAmount = timeEntry["RateOrAmount"]?.InnerText;
            var rateshourly = Convert.ToDouble(timeEntry.SelectSingleNode("*[@type='hourly']")?.InnerText);
            var ratesCustomerFee = Convert.ToDouble(timeEntry.SelectSingleNode("*[@type='customer_fee']")?.InnerText);
            var startDateTime = timeEntry["StartDateTime"]?.InnerText;
            var interimHourEntry = new CanonicalInterimHours
            {
                AccountNumber = !string.IsNullOrEmpty(accountNumber) ? accountNumber : null,
                JobCode = jobCode.Length > 50 ? jobCode.Substring(0, 50) : jobCode,
                RateOrAmount = !string.IsNullOrEmpty(rateOrAmount) ? Math.Round(Convert.ToDouble(rateshourly + ratesCustomerFee), 2) : (double?)null,
                ConsultantName = consultantName.Length > 255 ? consultantName.Substring(0, 255) : consultantName,
                Duration = !string.IsNullOrEmpty(duration) ? Math.Round(Convert.ToDouble(duration), 2) : (double?)null,
                Date = DateTime.TryParse(startDateTime, out var date) ? date.ToString("yyyy-MM-dd") : null,
                JobDescription = !string.IsNullOrEmpty(customerJobDescription) ? customerJobDescription.Length > 200 ? customerJobDescription.Substring(0, 200) : customerJobDescription : null,
                RevisionNumber = revisionNumber != null ? Convert.ToInt32(revisionNumber.InnerText) : (int?)null,
                RateType = !string.IsNullOrEmpty(rateType?.InnerText) ? rateType.InnerText : null
            };
            return interimHourEntry;
        }

        private static List<CanonicalInterimHours> GroupHoursByDateAndType(List<CanonicalInterimHours> interimHourList)
        {
            var dateGroups = interimHourList.GroupBy(x => new
            {
                x.Date,
                x.RateType
            });
            var summedDurationList = new List<CanonicalInterimHours>();
            foreach (var group in dateGroups)
            {
                var latestRevisionNumber = group.Max(x => x.RevisionNumber);
                var groupedList = group.First(x => x.RevisionNumber == latestRevisionNumber);
                var sum = group.Sum(x => x.Duration);
                groupedList.Duration = sum;
                summedDurationList.Add(groupedList);
            }
            return summedDurationList;
        }

        private static CanonicalInterimHoursValidationResult ValidateHours(List<CanonicalInterimHours> interimHours)
        {
            var validator = new CanonicalInterimHoursValidator();
            var validatedHours = new CanonicalInterimHoursValidationResult();
            foreach (var item in interimHours)
            {
                var result = validator.Validate(item);
                if (result.IsValid)
                {
                    validatedHours.Valid.Add(item);
                }
                else
                {
                    var invalidHours = new InvalidCanonicalInterimHours(item, result.Errors.Select(x => x.ErrorMessage).ToList());
                    validatedHours.Invalid.Add(invalidHours);
                }
            }
            return validatedHours;
        }
    }
}
