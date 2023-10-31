using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Netive.Functions.Models;
using NSubstitute;
using Xunit;

namespace Netive.Functions.UnitTests
{
    public class ValidateTimeEntriesTests
    {
        [Fact]
        public async Task Valid_XML_Time_Entry_Should_Return_Valid_Time_Entry()
        {
            var logger = Substitute.For<ILogger>();
            var requestBody = ValidTimeEntry;
            var requestMessage = GenerateHttpPostRequestMessage(requestBody);
            var response = await ValidateTimeEntries.Run(requestMessage, logger) as ObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid.Count.Should().Be(2);
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid.Count.Should().Be(0);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].ConsultantName.Should().Be("van Dam, (Stasya)");
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].Duration.Should().Be(4.0);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].RevisionNumber.Should().Be(2);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].RateType.Should().Be("RTH100");
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].ConsultantName.Should().Be("van Dam, (Stasya)");
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].Duration.Should().Be(4.0);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].RevisionNumber.Should().Be(2);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].RateType.Should().Be("RTH150");
        }

        [Fact]
        public async Task Test_Duration_Sum_Calculation_By_Rate_Type()
        {
            var logger = Substitute.For<ILogger>();
            var requestBody = durationSumTestEntry;
            var requestMessage = GenerateHttpPostRequestMessage(requestBody);
            var response = await ValidateTimeEntries.Run(requestMessage, logger) as ObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].Duration.Should().Be(5);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].RevisionNumber.Should().Be(2);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[0].RateType.Should().Be("RTH100");
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].Duration.Should().Be(3);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].RevisionNumber.Should().Be(2);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid[1].RateType.Should().Be("RTH150");
        }

        [Fact]
        public async Task Empty_Request_Should_Return_BadRequest()
        {
            var logger = Substitute.For<ILogger>();
            var requestBody = "";
            var requestMessage = GenerateHttpPostRequestMessage(requestBody);
            var response = await ValidateTimeEntries.Run(requestMessage, logger) as ObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            response.Value.ToString().Should().Contain("Invalid XML provided");
        }

        [Fact]
        public async Task Invalid_XML_Time_Entry_Should_Return_Invalid_Time_Entry()
        {
            var logger = Substitute.For<ILogger>();
            var requestBody = InvalidTimeEntry;
            var requestMessage = GenerateHttpPostRequestMessage(requestBody);
            var response = await ValidateTimeEntries.Run(requestMessage, logger) as ObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid.Count.Should().Be(0);
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid.Count.Should().Be(1);
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].CanonicalInterimHours.ConsultantName.Should().Be("Schmoe (Joe)");
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].CanonicalInterimHours.AccountNumber.Should().BeNull();
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].CanonicalInterimHours.RateOrAmount.Should().Be(125.0);
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].CanonicalInterimHours.JobCode.Should().Be("JOB000002");
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].CanonicalInterimHours.JobDescription.Length.Should().BeLessThan(201);
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].CanonicalInterimHours.Date.Should().BeNull();
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid[0].Reason.Should().Contain(new List<string>
            {
                "'Account Number' must not be empty.",
                "'Date' must not be empty.",
            });
        }

        [Fact]
        public async Task Invalid_XML_Time_Entry_If_Billable_True_Return_Nothing()
        {
            var logger = Substitute.For<ILogger>();
            var requestBody = InvalidTimeEntryButNotReturned;
            var requestMessage = GenerateHttpPostRequestMessage(requestBody);
            var response = await ValidateTimeEntries.Run(requestMessage, logger) as ObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            response.Value.As<CanonicalInterimHoursValidationResult>().Valid.Count.Should().Be(0);
            response.Value.As<CanonicalInterimHoursValidationResult>().Invalid.Count.Should().Be(0);
        }

        private static DefaultHttpRequest GenerateHttpPostRequestMessage(string requestBody)
        {
            var byteArray = Encoding.UTF8.GetBytes(requestBody);
            var stream = new MemoryStream(byteArray);
            var requestMessage = new DefaultHttpRequest(new DefaultHttpContext())
            {
                ContentType = "application/json",
                Body = stream
            };
            return requestMessage;
        }

        private const string ValidTimeEntry = @"<TimeCard xmlns=""http://ns.hr-xml.org/2007-04-15"">
    <Id validFrom=""2020-02-05"">
        <IdValue name=""recordid"">a1t0E0000029wVJQAY</IdValue>
    </Id>
    <ReportedResource>
        <Person>
            <Id>
                <IdValue name=""recordid"">a1d1r0000057ZPFAA2</IdValue>
            </Id>
            <PersonName>
                <FormattedName>van Dam, (Stasya)</FormattedName>
                <LegalName>Stasya van Dam</LegalName>
                <GivenName>Stasya</GivenName>
                <PreferredGivenName>Stasya</PreferredGivenName>
                <FamilyName primary=""true"">van Dam</FamilyName>
            </PersonName>
        </Person>
    </ReportedResource>
    <ReportedTime>
        <PeriodStartDate>2019-03-11</PeriodStartDate>
        <PeriodEndDate>2019-03-17</PeriodEndDate>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6EQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">2</IdValue>
                <IdValue name=""rateid"">RTH100</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-11</EndDateTime>
            <Duration>4</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6FQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">2</IdValue>
                <IdValue name=""rateid"">RTH150</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-13</EndDateTime>
            <Duration>4</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
    </ReportedTime>
    <SubmitterInfo>
        <Source>Nétive VMS Force 2 Cloud Edition</Source>
        <SubmittedDateTime>2020-02-14T17:14:38</SubmittedDateTime>
    </SubmitterInfo>
    <ApprovalInfo approverType=""x:SysJobAdmin|SysAllUsers"">
        <Person>
            <PersonName>
                <FormattedName>Kelly Ekeren (SAB)</FormattedName>
            </PersonName>
        </Person>
        <ApprovedDateTime>2020-02-06T07:22:08</ApprovedDateTime>
        <Comment />
    </ApprovalInfo>
    <AdditionalData>
        <StaffingAdditionalData>
            <CustomerReportingRequirements>
                <ManagerName>Rijdt van de, E. (Erna)</ManagerName>
                <ContactName>raet, i (intern)</ContactName>
                <PurchaseOrderNumber>test PO</PurchaseOrderNumber>
                <DepartmentCode>0015700002EKw0lAAD</DepartmentCode>
                <DepartmentName>4029673</DepartmentName>
                <LocationName>Amsterdam</LocationName>
                <CustomerJobCode>JOB108268</CustomerJobCode>
                <CustomerJobDescription>Stasya wordt ingezet als Salarisadministrateur</CustomerJobDescription>
                <AdditionalRequirement requirementTitle=""InclusiveRate"">True</AdditionalRequirement>
                <AdditionalRequirement requirementTitle=""assignmentreasonremarks"">test</AdditionalRequirement>
            </CustomerReportingRequirements>
            <ReferenceInformation>
                <PositionId>
                    <IdValue>unknown</IdValue>
                </PositionId>
                <IntermediaryId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">a161r00000Bem35AAB</IdValue>
                    <IdValue name=""externalid"">Raet Interim Services</IdValue>
                    <IdValue>Raet Resource Center</IdValue>
                </IntermediaryId>
                <StaffingSupplierId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">a1a1r00000O3J35AAF</IdValue>
                    <IdValue name=""externalid"">Raet Intern</IdValue>
                    <IdValue>Visma Raet Collega's</IdValue>
                </StaffingSupplierId>
                <StaffingCustomerId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">a0F1r000026rYLYEA2</IdValue>
                    <IdValue name=""externalid"">Acceptatie_Raet_Interim_Services</IdValue>
                    <IdValue>Visma | Raet Interim Services</IdValue>
                </StaffingCustomerId>
                <HumanResourceId>
                    <IdValue name=""recordid"">a1d1r0000057ZPFAA2</IdValue>
                </HumanResourceId>
                <AssignmentId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">a0j1r00000OCZBXAA5</IdValue>
                </AssignmentId>
                <StaffingCustomerOrgUnitId>
                    <IdValue name=""recordid"">a0G1r00001qQnQUEA0</IdValue>
                    <IdValue name=""externalid"">0015700002EKw0lAAD</IdValue>
                </StaffingCustomerOrgUnitId>
                <UserArea>
                    <RoleId>
                        <IdValue name=""recordid"">a0V1r00000KCWgAEAX</IdValue>
                    </RoleId>
                </UserArea>
            </ReferenceInformation>
        </StaffingAdditionalData>
    </AdditionalData>
    <UserArea>
        <ApprovedRevision xmlns=""http://vms.netive.nl/2016-08-02"">0</ApprovedRevision>
    </UserArea>
</TimeCard>";

        private const string durationSumTestEntry = @"<TimeCard xmlns=""http://ns.hr-xml.org/2007-04-15"">
    <Id validFrom=""2020-02-05"">
        <IdValue name=""recordid"">a1t0E0000029wVJQAY</IdValue>
    </Id>
    <ReportedResource>
        <Person>
            <Id>
                <IdValue name=""recordid"">a1d1r0000057ZPFAA2</IdValue>
            </Id>
            <PersonName>
                <FormattedName>van Dam, (Stasya)</FormattedName>
                <LegalName>Stasya van Dam</LegalName>
                <GivenName>Stasya</GivenName>
                <PreferredGivenName>Stasya</PreferredGivenName>
                <FamilyName primary=""true"">van Dam</FamilyName>
            </PersonName>
        </Person>
    </ReportedResource>
    <ReportedTime>
        <PeriodStartDate>2019-03-11</PeriodStartDate>
        <PeriodEndDate>2019-03-17</PeriodEndDate>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6EQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">0</IdValue>
                <IdValue name=""rateid"">RTH100</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-11</EndDateTime>
            <Duration>8</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6EQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">1</IdValue>
                <IdValue name=""rateid"">RTH100</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-11</EndDateTime>
            <Duration>-8</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6EQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">2</IdValue>
                <IdValue name=""rateid"">RTH100</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-11</EndDateTime>
            <Duration>5</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6FQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">0</IdValue>
                <IdValue name=""rateid"">RTH150</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-13</EndDateTime>
            <Duration>4</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
<TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6FQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">1</IdValue>
                <IdValue name=""rateid"">RTH150</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-13</EndDateTime>
            <Duration>-4</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
<TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">a1e0E000000ZA6FQAW</IdValue>
                <IdValue name=""createddate"">2020-02-06T07:21:50+01:00</IdValue>
                <IdValue name=""revision"">2</IdValue>
                <IdValue name=""rateid"">RTH150</IdValue>
            </Id>
            <StartDateTime>2019-03-11</StartDateTime>
            <EndDateTime>2019-03-13</EndDateTime>
            <Duration>3</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">50</RateOrAmount>
        </TimeInterval>
    </ReportedTime>
    <SubmitterInfo>
        <Source>Nétive VMS Force 2 Cloud Edition</Source>
        <SubmittedDateTime>2020-02-14T17:14:38</SubmittedDateTime>
    </SubmitterInfo>
    <ApprovalInfo approverType=""x:SysJobAdmin|SysAllUsers"">
        <Person>
            <PersonName>
                <FormattedName>Kelly Ekeren (SAB)</FormattedName>
            </PersonName>
        </Person>
        <ApprovedDateTime>2020-02-06T07:22:08</ApprovedDateTime>
        <Comment />
    </ApprovalInfo>
    <AdditionalData>
        <StaffingAdditionalData>
            <CustomerReportingRequirements>
                <ManagerName>Rijdt van de, E. (Erna)</ManagerName>
                <ContactName>raet, i (intern)</ContactName>
                <PurchaseOrderNumber>test PO</PurchaseOrderNumber>
                <DepartmentCode>0015700002EKw0lAAD</DepartmentCode>
                <DepartmentName>4029673</DepartmentName>
                <LocationName>Amsterdam</LocationName>
                <CustomerJobCode>JOB108268</CustomerJobCode>
                <CustomerJobDescription>Stasya wordt ingezet als Salarisadministrateur</CustomerJobDescription>
                <AdditionalRequirement requirementTitle=""InclusiveRate"">True</AdditionalRequirement>
                <AdditionalRequirement requirementTitle=""assignmentreasonremarks"">test</AdditionalRequirement>
            </CustomerReportingRequirements>
            <ReferenceInformation>
                <PositionId>
                    <IdValue>unknown</IdValue>
                </PositionId>
                <IntermediaryId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">a161r00000Bem35AAB</IdValue>
                    <IdValue name=""externalid"">Raet Interim Services</IdValue>
                    <IdValue>Raet Resource Center</IdValue>
                </IntermediaryId>
                <StaffingSupplierId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">a1a1r00000O3J35AAF</IdValue>
                    <IdValue name=""externalid"">Raet Intern</IdValue>
                    <IdValue>Visma Raet Collega's</IdValue>
                </StaffingSupplierId>
                <StaffingCustomerId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">a0F1r000026rYLYEA2</IdValue>
                    <IdValue name=""externalid"">Acceptatie_Raet_Interim_Services</IdValue>
                    <IdValue>Visma | Raet Interim Services</IdValue>
                </StaffingCustomerId>
                <HumanResourceId>
                    <IdValue name=""recordid"">a1d1r0000057ZPFAA2</IdValue>
                </HumanResourceId>
                <AssignmentId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">a0j1r00000OCZBXAA5</IdValue>
                </AssignmentId>
                <StaffingCustomerOrgUnitId>
                    <IdValue name=""recordid"">a0G1r00001qQnQUEA0</IdValue>
                    <IdValue name=""externalid"">0015700002EKw0lAAD</IdValue>
                </StaffingCustomerOrgUnitId>
                <UserArea>
                    <RoleId>
                        <IdValue name=""recordid"">a0V1r00000KCWgAEAX</IdValue>
                    </RoleId>
                </UserArea>
            </ReferenceInformation>
        </StaffingAdditionalData>
    </AdditionalData>
    <UserArea>
        <ApprovedRevision xmlns=""http://vms.netive.nl/2016-08-02"">0</ApprovedRevision>
    </UserArea>
</TimeCard>";

        private const string InvalidTimeEntry = @"
<TimeCard xmlns=""http://ns.hr-xml.org/2007-04-15"">
    <Id validFrom=""2019-10-24"">
        <IdValue name=""recordid"">someRandomness</IdValue>
    </Id>
    <ReportedResource>
        <Person>
            <Id>
                <IdValue name=""recordid"">someRandomness</IdValue>
            </Id>
            <PersonName>
                <FormattedName>Schmoe (Joe)</FormattedName>
                <LegalName>Joe Doe</LegalName>
                <GivenName>Joe</GivenName>
                <PreferredGivenName>Schmoe</PreferredGivenName>
                <MiddleName></MiddleName>
                <FamilyName primary=""true"">Schmoe</FamilyName>
            </PersonName>
        </Person>
    </ReportedResource>
    <ReportedTime>
        <PeriodStartDate>2019-10-23</PeriodStartDate>
        <PeriodEndDate>2019-10-27</PeriodEndDate>
        <TimeInterval type=""Regular"" billable=""true"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">someRandomness</IdValue>
                <IdValue name=""createddate"">2019-10-24T08:00:23+02:00</IdValue>
                <IdValue name=""revision"">1</IdValue>
                <IdValue name=""rateid"">RT13</IdValue>
            </Id>
            <Duration>-8</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">25</RateOrAmount>
            <RateOrAmount currency=""EUR"" type=""customer_fee"" multiplier=""100.00"">100</RateOrAmount>
            <RateOrAmount currency=""EUR"" type=""supplier_fee"" multiplier=""100.00"">26</RateOrAmount>
            <RateOrAmount currency=""EUR"" type=""payrate"">0</RateOrAmount>
        </TimeInterval>
    </ReportedTime>
    <SubmitterInfo>
        <Source>Nétive VMS Force 2 Cloud Edition</Source>
        <SubmittedDateTime>2019-10-24T08:38:07</SubmittedDateTime>
    </SubmitterInfo>
    <AdditionalData>
        <StaffingAdditionalData>
            <CustomerReportingRequirements>
                <ManagerName>Smith (John)</ManagerName>
                <ContactName>Aplessed (Alice)</ContactName>				
                <LocationName>Riga</LocationName>
                <CostCenterCode>CU-01</CostCenterCode>
                <CostCenterName>DEV</CostCenterName>
                <CustomerJobCode>JOB000002</CustomerJobCode>
                <AdditionalRequirement requirementTitle=""InclusiveRate"">True</AdditionalRequirement>
                <CustomerJobDescription>DevOooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooops</CustomerJobDescription>
            </CustomerReportingRequirements>
            <ReferenceInformation>
                <OrderId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </OrderId>
                <PositionId>
                    <IdValue>unknown</IdValue>
                </PositionId>
                <IntermediaryId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                    <IdValue name=""externalid"">MSPFBU1</IdValue>
                    <IdValue>FB Unit 1</IdValue>
                </IntermediaryId>
                <StaffingSupplierId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                    <IdValue name=""externalid"">LEVDSHQ</IdValue>
                    <IdValue>DetachStunter</IdValue>
                </StaffingSupplierId>
                <StaffingCustomerId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                    <IdValue name=""externalid"">AVANSYS</IdValue>
                    <IdValue>Avansis Automatisering</IdValue>
                </StaffingCustomerId>
                <MasterOrderId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </MasterOrderId>
                <HumanResourceId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </HumanResourceId>
                <AssignmentId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </AssignmentId>
                <StaffingSupplierOrgUnitId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </StaffingSupplierOrgUnitId>
                <StaffingCustomerOrgUnitId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </StaffingCustomerOrgUnitId>
                <UserArea>
                    <RoleId>
                        <IdValue name=""recordid"">someRandomness</IdValue>
                        <IdValue name=""externalid"">RAA1</IdValue>
                    </RoleId>
                    <ApprovedRevision>1337</ApprovedRevision>
                </UserArea>
            </ReferenceInformation>
        </StaffingAdditionalData>
    </AdditionalData>
     <UserArea>
        <ApprovedRevision xmlns=""http://vms.netive.nl/2016-08-02"">999</ApprovedRevision>
    </UserArea>
</TimeCard>";

        private const string InvalidTimeEntryButNotReturned = @"
<TimeCard xmlns=""http://ns.hr-xml.org/2007-04-15"">
    <Id validFrom=""2019-10-24"">
        <IdValue name=""recordid"">someRandomness</IdValue>
    </Id>
    <ReportedResource>
        <Person>
            <Id>
                <IdValue name=""recordid"">someRandomness</IdValue>
            </Id>
            <PersonName>
                <FormattedName>Schmoe (Joe)</FormattedName>
                <LegalName>Joe Doe</LegalName>
                <GivenName>Joe</GivenName>
                <PreferredGivenName>Schmoe</PreferredGivenName>
                <MiddleName></MiddleName>
                <FamilyName primary=""true"">Schmoe</FamilyName>
            </PersonName>
        </Person>
    </ReportedResource>
    <ReportedTime>
        <PeriodStartDate>2019-10-23</PeriodStartDate>
        <PeriodEndDate>2019-10-27</PeriodEndDate>
        <TimeInterval type=""Regular"" billable=""false"" actionCode=""Add"">
            <Id>
                <IdValue name=""recordid"">someRandomness</IdValue>
                <IdValue name=""createddate"">2019-10-24T08:00:23+02:00</IdValue>
                <IdValue name=""revision"">1</IdValue>
                <IdValue name=""rateid"">RT13</IdValue>
            </Id>
            <Duration>-8</Duration>
            <RateOrAmount currency=""EUR"" type=""hourly"" multiplier=""100.00"" toBeBilled=""true"" toBePaid=""true"">25</RateOrAmount>
            <RateOrAmount currency=""EUR"" type=""customer_fee"" multiplier=""100.00"">0</RateOrAmount>
            <RateOrAmount currency=""EUR"" type=""supplier_fee"" multiplier=""100.00"">0</RateOrAmount>
            <RateOrAmount currency=""EUR"" type=""payrate"">0</RateOrAmount>
        </TimeInterval>
    </ReportedTime>
    <SubmitterInfo>
        <Source>Nétive VMS Force 2 Cloud Edition</Source>
        <SubmittedDateTime>2019-10-24T08:38:07</SubmittedDateTime>
    </SubmitterInfo>
    <AdditionalData>
        <StaffingAdditionalData>
            <CustomerReportingRequirements>
                <ManagerName>Smith (John)</ManagerName>
                <ContactName>Aplessed (Alice)</ContactName>				
                <LocationName>Riga</LocationName>
                <CostCenterCode>CU-01</CostCenterCode>
                <CostCenterName>DEV</CostCenterName>
                <CustomerJobCode>JOB000002</CustomerJobCode>
                <AdditionalRequirement requirementTitle=""InclusiveRate"">True</AdditionalRequirement>
                <CustomerJobDescription>DevOooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooops</CustomerJobDescription>
            </CustomerReportingRequirements>
            <ReferenceInformation>
                <OrderId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </OrderId>
                <PositionId>
                    <IdValue>unknown</IdValue>
                </PositionId>
                <IntermediaryId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                    <IdValue name=""externalid"">MSPFBU1</IdValue>
                    <IdValue>FB Unit 1</IdValue>
                </IntermediaryId>
                <StaffingSupplierId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                    <IdValue name=""externalid"">LEVDSHQ</IdValue>
                    <IdValue>DetachStunter</IdValue>
                </StaffingSupplierId>
                <StaffingCustomerId idOwner=""StaffingCompany"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                    <IdValue name=""externalid"">AVANSYS</IdValue>
                    <IdValue>Avansis Automatisering</IdValue>
                </StaffingCustomerId>
                <MasterOrderId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </MasterOrderId>
                <HumanResourceId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </HumanResourceId>
                <AssignmentId idOwner=""StaffingCustomer"">
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </AssignmentId>
                <StaffingSupplierOrgUnitId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </StaffingSupplierOrgUnitId>
                <StaffingCustomerOrgUnitId>
                    <IdValue name=""recordid"">someRandomness</IdValue>
                </StaffingCustomerOrgUnitId>
                <UserArea>
                    <RoleId>
                        <IdValue name=""recordid"">someRandomness</IdValue>
                        <IdValue name=""externalid"">RAA1</IdValue>
                    </RoleId>
                    <ApprovedRevision>1337</ApprovedRevision>
                </UserArea>
            </ReferenceInformation>
        </StaffingAdditionalData>
    </AdditionalData>
     <UserArea>
        <ApprovedRevision xmlns=""http://vms.netive.nl/2016-08-02"">999</ApprovedRevision>
    </UserArea>
</TimeCard>";
    }
}

