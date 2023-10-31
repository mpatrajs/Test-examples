using System;
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
    public class ValidateCustomersTests
    {
        private readonly ILogger _logger = Substitute.For<ILogger>();

        [Fact]
        public void Should_Return_Valid_Customers()
        {
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(ComposeValidInput()));
            var response = ValidateCustomers.Run(requestMessage, _logger) as ObjectResult;

            response.Value.As<CustomerValidationResult>().Valid[0].CustomerId.Should().Be("1234567890");
            response.Value.As<CustomerValidationResult>().Valid[0].SalesAccountId.Should().Be("0015E00000llH8QQAU");
            response.Value.As<CustomerValidationResult>().Valid[0].CustomerName.Should().Be("Test");
            response.Value.As<CustomerValidationResult>().Valid[0].ServiceDeskUnit.Should().Be("1;Zorg & Welzijn");
            response.Value.As<CustomerValidationResult>().Valid[0].TeamBPO.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].OwnerName.Should().Be("Name Surname");
            response.Value.As<CustomerValidationResult>().Valid[0].Market.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].CustomerServiceSupplier.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].Status.Should().Be("New Customer");
            response.Value.As<CustomerValidationResult>().Valid[0].Active.Should().Be("Active");
            response.Value.As<CustomerValidationResult>().Valid[0].PhoneNumber.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].Website.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialStreet.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialPostalCode.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialCity.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialRegionCode.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialRegionName.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialCountryCodeAlpha2.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].ResidentialCountryName.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalStreet.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalPostalCode.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalCity.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalRegionCode.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalRegionName.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalCountryCodeAlpha2.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].PostalCountryName.Should().Be(null);
            response.Value.As<CustomerValidationResult>().Valid[0].CreationDate.Should().Be(Convert.ToDateTime("2019-09-18T00:00:00"));
            response.Value.As<CustomerValidationResult>().Invalid.Should().BeEmpty();
        }

        [Fact]
        public void Should_Return_Invalid_Customers()
        {
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(ComposeInvalidInput()));
            var response = ValidateCustomers.Run(requestMessage, _logger) as ObjectResult;

            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field CustomerId from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field SalesAccountId from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field CustomerName from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field OwnerName from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field Status from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field Active from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Invalid[0].Reason.Should().Contain("Required field CreationDate from SalesForce is null or empty.");
            response.Value.As<CustomerValidationResult>().Valid.Should().BeEmpty();
        }

        private static List<Customer> ComposeValidInput()
        {
            var customerList = new List<Customer>
            {
                new Customer
                {
                    CustomerId = "1234567890",
                    SalesAccountId = "0015E00000llH8QQAU",
                    CustomerName = "Test",
                    ServiceDeskUnit = "1;Zorg & Welzijn",
                    TeamBPO = null,
                    OwnerName = "Name Surname",
                    Market = null,
                    CustomerServiceSupplier = null,
                    Status = "New Customer",
                    Active = "Active",
                    PhoneNumber = null,
                    Website = null,
                    ResidentialStreet = null,
                    ResidentialPostalCode = null,
                    ResidentialCity = null,
                    ResidentialRegionCode = null,
                    ResidentialRegionName = null,
                    ResidentialCountryCodeAlpha2 = null,
                    ResidentialCountryName = null,
                    PostalStreet = null,
                    PostalPostalCode = null,
                    PostalCity = null,
                    PostalRegionCode = null,
                    PostalRegionName = null,
                    PostalCountryCodeAlpha2 = null,
                    PostalCountryName = null,
                    CreationDate = Convert.ToDateTime("2019-09-18T00:00:00")
                }
            };
            return customerList;
        }

        private static List<Customer> ComposeInvalidInput()
        {
            var customerList = new List<Customer>
            {
                new Customer
                {
                    CustomerId = null,
                    SalesAccountId = null,
                    CustomerName = null,
                    ServiceDeskUnit = null,
                    TeamBPO = null,
                    OwnerName = null,
                    Market = null,
                    CustomerServiceSupplier = null,
                    Status = null,
                    Active = null,
                    PhoneNumber = null,
                    Website = null,
                    ResidentialStreet = null,
                    ResidentialPostalCode = null,
                    ResidentialCity = null,
                    ResidentialRegionCode = null,
                    ResidentialRegionName = null,
                    ResidentialCountryCodeAlpha2 = null,
                    ResidentialCountryName = null,
                    PostalStreet = null,
                    PostalPostalCode = null,
                    PostalCity = null,
                    PostalRegionCode = null,
                    PostalRegionName = null,
                    PostalCountryCodeAlpha2 = null,
                    PostalCountryName = null,
                    CreationDate = null
                }
            };
            return customerList;
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