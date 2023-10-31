using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Salesforce.Functions.Models;
using Xunit;

namespace Salesforce.Functions.UnitTests
{
    public class ValidateYouForceCustomerContactsTests
    {
        private readonly ILogger _logger = Substitute.For<ILogger>();

        [Fact]
        public void ValidYouForceCustomerContact_Should_Return_Valid_YouForceCustomerContact()
        {
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(ComposeValidInput()));
            var response = ValidateCustomerContactsForYouForce.Run(requestMessage, _logger) as ObjectResult;

            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].FamilyName.Should().Be("Braun");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].Id.Should().Be("8r876rkjhg");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].WorkPhone1.Should().Be("+31513636183");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].WorkPhone2.Should().Be("+3154296493");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].Email.Should().Be("helga.braun27@accell.nl");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].Initials.Should().Be("H.I.J.");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].Gender.Should().Be("F");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].SupportRolesCount.Should().Be(1);
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].InvoicingContact.Should().Be(false);
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].DataProtectionContact.Should().Be(true);
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].Title.Should().Be(null);
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].JobTitle.Should().Be("Sales Representative");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].JobTitleCategory.Should().Be("Account Manager");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid[0].TenantNumber.Should().Be("4029148");
            response.Value.As<YouForceCustomerContactValidationResult>().Invalid.Should().BeEmpty();
        }

        [Fact]
        public void InvalidYouForceCustomerContact_Should_Return_Invalid_YouForceCustomerContact()
        {
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(ComposeInvalidInput()));
            var response = ValidateCustomerContactsForYouForce.Run(requestMessage, _logger) as ObjectResult;

            response.Value.As<YouForceCustomerContactValidationResult>().Invalid[0].Reason.Should().Contain("Required field SupportRolesCount from SalesForce is null or empty.");
            response.Value.As<YouForceCustomerContactValidationResult>().Invalid[0].Reason.Should().Contain("Required field DataProtectionContact from SalesForce is null or empty.");
            response.Value.As<YouForceCustomerContactValidationResult>().Invalid[0].Reason.Should().Contain("Required field InvoicingContact from SalesForce is null or empty.");
            response.Value.As<YouForceCustomerContactValidationResult>().Invalid[0].Reason.Should().Contain("Required field FamilyName from SalesForce is null or empty.");
            response.Value.As<YouForceCustomerContactValidationResult>().Valid.Should().BeEmpty();
        }

        private static List<YouForceCustomerContact> ComposeValidInput()
        {
            var customerContactsList = new List<YouForceCustomerContact>
            {
                new YouForceCustomerContact
                {
                    FamilyName = "Braun",
                    Id = "8r876rkjhg",
                    WorkPhone1 = "+31513636183",
                    WorkPhone2 = "+3154296493",
                    Email = "helga.braun27@accell.nl",
                    Initials = "H.I.J.",
                    Gender = "F",
                    SupportRolesCount = 1,
                    InvoicingContact = false,
                    DataProtectionContact = true,
                    Title = null,
                    JobTitle = "Sales Representative",
                    JobTitleCategory = "Account Manager",
                    TenantNumber = "4029148"
                }
            };
            return customerContactsList;
        }

        private static List<YouForceCustomerContact> ComposeInvalidInput()
        {
            var customerContactsList = new List<YouForceCustomerContact>
            {
                new YouForceCustomerContact
                {
                    Id = "8r876rkjhg",
                    WorkPhone1 = "+31513636183",
                    WorkPhone2 = "+3154296493",
                    Email = "helga.braun27@accell.nl",
                    Initials = "H.I.J.",
                    Gender = "F",
                    Title = null,
                    JobTitle = "Sales Representative",
                    JobTitleCategory = "Account Manager",
                    TenantNumber = "4029148"
                }
            };
            return customerContactsList;
        }

        private static DefaultHttpRequest GenerateHttpPostRequestMessage(string input)
        {
            var byteArray = Encoding.UTF8.GetBytes(input);
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