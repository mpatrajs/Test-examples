using System.IO;
using System.Text;
using System.Threading.Tasks;
using Capman.Functions.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Capman.Functions.UnitTests
{
    public class ValidateAndConvertTimeEntriesCsvTests
    {
        private readonly ILogger _logger = Substitute.For<ILogger>();

        [Fact]
        public async Task ValidateAndConvertTimeEntriesCsv_WithoutTimeEntryLines_ReturnsEmptyTimeEntryValidationResults()
        {
            var requestMessage = GenerateHttpPostRequestMessage();

            var response = await ValidateAndConvertTimeEntriesCsv.RunAsync(requestMessage, _logger) as ObjectResult;

            response.Value.As<TimeEntryValidationResult>().Valid.Count.Should().Be(0);
            response.Value.As<TimeEntryValidationResult>().Invalid.Count.Should().Be(0);
        }

        [Fact]
        public async Task ValidateAndConvertTimeEntriesCsv_WithOnlyValidTimeEntryLines_ReturnsOnlyCanonicalTimeEntries()
        {
            var requestMessage = GenerateHttpPostRequestMessage(
                "3901376;11-10-2019;0;01;202;652;01;Andre werk controle KPN indien van toepassing;2938291;0.5;Test Employee"
            );

            var response = await ValidateAndConvertTimeEntriesCsv.RunAsync(requestMessage, _logger) as ObjectResult;

            response.Value.As<TimeEntryValidationResult>().Valid[0].Should().BeOfType<CanonicalTimeEntry>();
            response.Value.As<TimeEntryValidationResult>().Valid[0].EmployeeId.Should().Be("3901376");
            response.Value.As<TimeEntryValidationResult>().Invalid.Count.Should().Be(0);
        }

        [Fact]
        public async Task ValidateAndConvertTimeEntriesCsv_WithOnlyTimeEntryContainingRobotEmployeeID_ReturnsEmptyTimeEntryValidationResults()
        {
            var requestMessage = GenerateHttpPostRequestMessage(
                "1111111113;11-10-2019;0;01;202;652;01;Andre werk controle KPN indien van toepassing;2938291;0.5;Test Employee"
            );

            var response = await ValidateAndConvertTimeEntriesCsv.RunAsync(requestMessage, _logger) as ObjectResult;

            response.Value.As<TimeEntryValidationResult>().Valid.Count.Should().Be(0);
            response.Value.As<TimeEntryValidationResult>().Invalid.Count.Should().Be(0);
        }

        [Fact]
        public async Task ValidateAndConvertTimeEntriesCsv_WithOnlyInvalidTimeEntryLines_ReturnsOnlyTimeEntryValidationFailedResults()
        {
            var requestMessage = GenerateHttpPostRequestMessage(
                "3901376;11-10-2019;0;01;202;652;01;Andre werk controle KPN indien van toepassing;;0.5;Test Employee"
            );

            var response = await ValidateAndConvertTimeEntriesCsv.RunAsync(requestMessage, _logger) as ObjectResult;

            response.Value.As<TimeEntryValidationResult>().Invalid[0].Should().BeOfType<TimeEntryValidationFailedResult>();
            response.Value.As<TimeEntryValidationResult>().Valid.Count.Should().Be(0);
        }

        private static DefaultHttpRequest GenerateHttpPostRequestMessage(string timeEntryLine = "", string timeEntryLine2 = "")
        {
            var csv = new StringBuilder();
            var header = "Employee ID;Date;Project;Project phase;Activity;Cost Center;Activity group;Comment;Capman Line ID;Hours;Employee Name";
            csv.AppendLine(header);
            csv.AppendLine(timeEntryLine);
            csv.AppendLine(timeEntryLine2);
            var byteArray = Encoding.UTF8.GetBytes(csv.ToString());
            var stream = new MemoryStream(byteArray);

            var requestMessage = new DefaultHttpRequest(new DefaultHttpContext())
            {
                ContentType = "text/csv",
                Body = stream
            };
            return requestMessage;
        }
    }
}
