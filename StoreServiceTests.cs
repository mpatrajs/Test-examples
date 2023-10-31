using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mystore.Functions.Helpers;
using Mystore.Functions.Interfaces;
using Mystore.Functions.Models;
using NSubstitute;
using Xunit;

namespace Mystore.Functions.UnitTests
{
   public class StoreServiceTests
   {
      private readonly StoreService _sut;
      private readonly IStoreRepository _storeService = Substitute.For<IStoreRepository>();

      public StoreServiceTests() => _sut = new StoreService(_storeService);

      [Fact]
      public async Task GetAllStoresAsync_ShouldReturnStores_WhenTheyExist()
      {
         var storeName = "teststore_mystore_no";
         var storeQueryResult = new List<GeneralInfo>
         {
            new GeneralInfo
            {
               User = storeName
            }
         };
         _storeService.GetAllStoresAsync().Returns(storeQueryResult);
         var stores = await _sut.GetAllStoresAsync();
         Assert.Equal(stores.FirstOrDefault(), storeName);
      }

      [Fact]
      public async Task GetAllStoresAsync_ShouldReturnEmpty_WhenNoResults()
      {
         var storeQueryResult = new List<GeneralInfo>();
         _storeService.GetAllStoresAsync().Returns(storeQueryResult);
         var stores = await _sut.GetAllStoresAsync();
         Assert.Empty(stores);
      }

      [Fact]
      public async Task GetSingleStoreStatsAsync_ShouldReturnStoreStats_WhenItExists()
      {
         var storeName = "teststore_mystore_no";
         var templateName = "america";

         var storeQueryResult = new SingleStoreQueryResult
         {
            StoreDomainInfo = new DomainInfo
            {
               Domain = "Test Domain"
            },
            StoreGeneralStats = new GeneralInfo
            {
               User = storeName,
               DateSignedUp = DateTime.MinValue
            },
            StoreConfigStats = new List<StoreConfiguration>
            {
               new()
               {
                  ConfigurationKey = "TEMPLATE_NAME",
                  ConfigurationValue = templateName
               }
            },
            StoreOrderRevenuePosStats = new OrderRevenuePos
            {
            },
            Contacts = new List<Contact>()
         };
         _storeService.GetStoreStatsAsync(storeName).Returns(storeQueryResult);
         var store = await _sut.GetSingleStoreStatsAsync(storeName);
         Assert.Equal("Test Domain", store.Domains);
         Assert.Equal(storeName, store.General.User);
         Assert.Equal(templateName, store.Configuration.Templates);
         Assert.Null(store.General.DateSignedUp);
      }

      [Fact]
      public async Task GetSingleStoreStatsAsync_ShouldReturnStoreStatsWithCorrectValues_WhenItExists()
      {
         var storeName = "teststore_mystore_no";
         var templateName = "america";

         var storeQueryResult = new SingleStoreQueryResult
         {
            StoreDomainInfo = new DomainInfo
            {
               Domain = "Test Primary Domain"
            },
            StoreGeneralStats = new GeneralInfo
            {
               User = storeName,
               DateSignedUp = DateTime.MinValue,
               DeletionDate = null,
               CustomerNumber = 112233,
               ActiveCustomer = true,
               Address = "Address 123",
               AdminCount = 5,
               CmrAiChurn = false,
               CmrAiPaymentFrequency = "0",
               CmrrChurn = false,
               CmrrPaymentFrequency = "0",
               CompanyName = "Test Company AS",
               DigitalMarketingChurn = false,
               DigitalMarketingPaymentFrequency = "0",
               EstoreHostingChurn = false,
               EstoreHostingPaymentFrequency = "0",
               OrdersCountLast30Days = 30,
               OrdersCountLast365Days = 1365,
               OrdersCountLast3Months = 300,
               OrdersCountLast6Months = 600,
               OrdersCountLast7Days = 7,
               OrdersCountLast9Months = 900,
               OrdersCountLastDay = 1,
               OrdersCountYearToDate = 1000,
               OrganizationNumber = "123456789",
               Pakke = 12,
               Phone = "123456789",
               PosHostingChurn = false,
               PosHostingPaymentFrequency = "0",
               PostalCode = "123",
               RecurringPos = false,
               RecurringCrall = false,
               RecurringEStore = false,
               RecurringLogistics = false,
               RecurringMarketing = false,
               RevenueLastDay = 100,
               RevenueLast7Days = 300,
               RevenueLast30Days = 300,
               RevenueLast3Months = 300,
               RevenueLast6Months = 600,
               RevenueLast9Months = 900,
               RevenueLast365Days = 1365,
               RevenueYearToDate = 1000,
               RevenueTotal = 2000,
               Segment = "F"
            },
            StoreOrderCountStats = new OrderTotalCount
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               TotalOrdersCount = 10000,
               VippsCount = 2000
            },
            StoreOrderLastDayStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderLast7DaysStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderLast30DaysStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderLast3MonthsStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderLast6MonthsStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderLast9MonthsStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderLast365DaysStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderYearToDateStats = new OrderInfoCountByInterval
            {
               AdyenCount = 2000,
               KlarnaCount = 2000,
               KlarnaNativeCount = 2000,
               KlarnaV3Count = 2000,
               VippsCount = 2000
            },
            StoreOrderAverageLinesPerOrderStats = new OrderAverageLinesPerOrder
            {
               AverageLinesPerOrderYesterday = 2000,
               AverageLinesPerOrderLast7Days = 2000,
               AverageLinesPerOrderLast30Days = 2000,
               AverageLinesPerOrderLast3Months = 2000,
               AverageLinesPerOrderLast6Months = 2000,
               AverageLinesPerOrderLast9Months = 2000,
               AverageLinesPerOrderLast365Days = 2000,
               AverageLinesPerOrderYearToDate = 2000,
               AverageLinesPerOrderTotal = 2000
            },
            StoreCreditStats = new CreditHistoryInfo
            {
               CreditsAvailable = 2000,
               CreditsUsedYesterday = -1000,
               CreditsUsedLast7Days = -1000,
               CreditsUsedLast30Days = -1000,
               CreditsUsedLast3Months = -1000,
               CreditsUsedLast6Months = -1000,
               CreditsUsedLast9Months = -1000,
               CreditsUsedLast365Days = -1000,
               CreditsUsedYearToDate = -1000
            },
            StoreProductStats = new ProductsInfo
            {
               Products = 2000,
               ProductVariants = 10000
            },
            StoreOrderInvoiceStats = new InvoiceInfo
            {
               FraktSentRevenueLastDay = 2000,
               FraktSentRevenueLast7Days = 2000,
               FraktSentRevenueLast30Days = 2000,
               FraktSentRevenueLast3Months = 2000,
               FraktSentRevenueLast6Months = 2000,
               FraktSentRevenueLast9Months = 2000,
               FraktSentRevenueLast365Days = 2000,
               FraktSentRevenueYearToDate = 2000,
               FraktSentRevenueTotal = 2000,
               LastPostnordInvoiceDate = DateTime.MinValue,
               Mystorefrakt = true,
               PackagesSentLastDay = 2000,
               PackagesSentLast7Days = 2000,
               PackagesSentLast30Days = 2000,
               PackagesSentLast3Months = 2000,
               PackagesSentLast6Months = 2000,
               PackagesSentLast9Months = 2000,
               PackagesSentLast365Days = 2000,
               PackagesSentYearToDate = 2000,
               PackagesSentTotal = 2000
            },
            StoreConfigStats = new List<StoreConfiguration>
            {
               new()
               {
                  ConfigurationKey = "TEMPLATE_NAME",
                  ConfigurationValue = templateName
               },
               new()
               {
                  ConfigurationKey = "API_ACTIVE",
                  ConfigurationValue = "true"
               },
               new()
               {
                  ConfigurationKey = "DEFAULT_ORDERS_CANCELED_ID",
                  ConfigurationValue = "4"
               }
            },
            StoreOrderRevenuePosStats = new OrderRevenuePos
            {
               RevenuePosLastDay = 0,
               RevenuePosLast7Days = 50,
               RevenuePosLast30Days = 100,
               RevenuePosLast3Months = 100,
               RevenuePosLast6Months = 100,
               RevenuePosLast9Months = 100,
               RevenuePosLast365Days = 200,
               RevenuePosYearToDate = 200,
               RevenuePosTotal = 300
            },
            Contacts = new List<Contact>()
         };
         _storeService.GetStoreStatsAsync(storeName).Returns(storeQueryResult);
         var store = await _sut.GetSingleStoreStatsAsync(storeName);
         Assert.True(store.General.ActiveCustomer);
         Assert.Equal("Test Primary Domain", store.Domains);
         Assert.Equal(112233, store.General.CustomerNumber);
         Assert.Equal("Address 123", store.General.Address);
         Assert.Equal(5, store.General.AdminCount);
         Assert.False(store.General.CmrAiChurn);
         Assert.Equal("0", store.General.CmrAiPaymentFrequency);
         Assert.False(store.General.CmrrChurn);
         Assert.Equal("0", store.General.CmrrPaymentFrequency);
         Assert.Equal("Test Company AS", store.General.CompanyName);
         Assert.Null(store.General.DateSignedUp);
         Assert.False(store.General.DigitalMarketingChurn);
         Assert.Equal("0", store.General.DigitalMarketingPaymentFrequency);
         Assert.False(store.General.EstoreHostingChurn);
         Assert.Equal("0", store.General.EstoreHostingPaymentFrequency);
         Assert.Equal(1, store.General.OrdersCountLastDay);
         Assert.Equal(7, store.General.OrdersCountLast7Days);
         Assert.Equal(30, store.General.OrdersCountLast30Days);
         Assert.Equal(300, store.General.OrdersCountLast3Months);
         Assert.Equal(600, store.General.OrdersCountLast6Months);
         Assert.Equal(900, store.General.OrdersCountLast9Months);
         Assert.Equal(1365, store.General.OrdersCountLast365Days);
         Assert.Equal(1000, store.General.OrdersCountYearToDate);
         Assert.Equal("123456789", store.General.OrganizationNumber);
         Assert.Equal(12, store.General.Pakke);
         Assert.Equal("123456789", store.General.Phone);
         Assert.False(store.General.PosHostingChurn);
         Assert.Equal("0", store.General.PosHostingPaymentFrequency);
         Assert.Equal("123", store.General.PostalCode);
         Assert.False(store.General.RecurringCrall);
         Assert.False(store.General.RecurringEStore);
         Assert.False(store.General.RecurringLogistics);
         Assert.False(store.General.RecurringMarketing);
         Assert.False(store.General.RecurringPos);
         Assert.Equal(100, store.General.RevenueLastDay);
         Assert.Equal(250, store.General.RevenueLast7Days);
         Assert.Equal(200, store.General.RevenueLast30Days);
         Assert.Equal(200, store.General.RevenueLast3Months);
         Assert.Equal(500, store.General.RevenueLast6Months);
         Assert.Equal(800, store.General.RevenueLast9Months);
         Assert.Equal(1165, store.General.RevenueLast365Days);
         Assert.Equal(800, store.General.RevenueYearToDate);
         Assert.Equal(1700, store.General.RevenueTotal);
         Assert.Equal("F", store.General.Segment);
         Assert.Equal(storeName, store.General.User);

         Assert.Equal(2000, store.ModuleOrderTotalCount.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderTotalCount.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderTotalCount.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderTotalCount.KlarnaV3Count);
         Assert.Equal(10000, store.ModuleOrderTotalCount.TotalOrdersCount);
         Assert.Equal(2000, store.ModuleOrderTotalCount.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLastDay.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLastDay.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLastDay.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLastDay.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLastDay.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLast7Days.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLast7Days.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLast7Days.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLast7Days.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLast7Days.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLast30Days.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLast30Days.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLast30Days.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLast30Days.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLast30Days.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLast3Months.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLast3Months.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLast3Months.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLast3Months.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLast3Months.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLast6Months.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLast6Months.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLast6Months.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLast6Months.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLast6Months.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLast9Months.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLast9Months.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLast9Months.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLast9Months.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLast9Months.VippsCount);

         Assert.Equal(2000, store.ModuleOrderLast365Days.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderLast365Days.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderLast365Days.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderLast365Days.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderLast365Days.VippsCount);

         Assert.Equal(2000, store.ModuleOrderYearToDate.AdyenCount);
         Assert.Equal(2000, store.ModuleOrderYearToDate.KlarnaCount);
         Assert.Equal(2000, store.ModuleOrderYearToDate.KlarnaNativeCount);
         Assert.Equal(2000, store.ModuleOrderYearToDate.KlarnaV3Count);
         Assert.Equal(2000, store.ModuleOrderYearToDate.VippsCount);

         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderYesterday);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderLast7Days);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderLast30Days);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderLast3Months);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderLast6Months);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderLast9Months);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderLast365Days);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderYearToDate);
         Assert.Equal(2000, store.OrderAverageLinesPerOrder.AverageLinesPerOrderTotal);

         Assert.Equal(2000, store.CreditHistory.CreditsAvailable);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedYesterday);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedLast7Days);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedLast30Days);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedLast3Months);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedLast6Months);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedLast9Months);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedLast365Days);
         Assert.Equal(-1000, store.CreditHistory.CreditsUsedYearToDate);

         Assert.Equal(2000, store.Products.Products);
         Assert.Equal(10000, store.Products.ProductVariants);

         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLastDay);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLast7Days);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLast30Days);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLast3Months);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLast6Months);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLast9Months);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueLast365Days);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueYearToDate);
         Assert.Equal(2000, store.InvoiceInfo.FraktSentRevenueTotal);
         Assert.Equal(DateTime.MinValue, store.InvoiceInfo.LastPostnordInvoiceDate);
         Assert.True(store.InvoiceInfo.Mystorefrakt);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLastDay);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLast7Days);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLast30Days);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLast3Months);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLast6Months);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLast9Months);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentLast365Days);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentYearToDate);
         Assert.Equal(2000, store.InvoiceInfo.PackagesSentTotal);

         Assert.Equal(0, store.OrderRevenuePos.RevenuePosLastDay);
         Assert.Equal(50, store.OrderRevenuePos.RevenuePosLast7Days);
         Assert.Equal(100, store.OrderRevenuePos.RevenuePosLast30Days);
         Assert.Equal(100, store.OrderRevenuePos.RevenuePosLast3Months);
         Assert.Equal(100, store.OrderRevenuePos.RevenuePosLast6Months);
         Assert.Equal(100, store.OrderRevenuePos.RevenuePosLast9Months);
         Assert.Equal(200, store.OrderRevenuePos.RevenuePosLast365Days);
         Assert.Equal(200, store.OrderRevenuePos.RevenuePosYearToDate);
         Assert.Equal(300, store.OrderRevenuePos.RevenuePosTotal);

         Assert.Equal(";API_ACTIVE", store.Configuration.InstalledApps);
         Assert.Equal(4, store.Configuration.DefaultOrdersCanceledId);
         Assert.False(store.Configuration.CrallStatus);
         Assert.False(store.Configuration.CrallSearch);
         Assert.False(store.Configuration.CrallRecommendation);
         Assert.Equal("america", store.Configuration.Templates);
         Assert.False(store.Configuration.KlarnaInstalled);
      }

      [Fact]
      public void GetStoreConfigByStoreName_ShouldReturnApps_WhenTheyExist()
      {
         var storeConfiguration = new List<StoreConfiguration>
         {
            new()
            {
               ConfigurationKey = "DIGITAL_PRODUCTS_ACTIVE",
               ConfigurationValue = ""
            },
            new()
            {
               ConfigurationKey = "DEFAULT_ORDERS_CANCELED_ID",
               ConfigurationValue = "4"
            },
            new()
            {
               ConfigurationKey = "KLARNA_CHECKOUT_V3_STATUS",
               ConfigurationValue = ""
            },
            new()
            {
               ConfigurationKey = "CRALL_SEARCH",
               ConfigurationValue = ""
            }
         };
         var configurationInfo = _sut.GetStoreConfigByStoreName(storeConfiguration);
         Assert.False(configurationInfo.CrallRecommendation);
         Assert.True(configurationInfo.CrallSearch);
         Assert.True(configurationInfo.KlarnaInstalled);
         Assert.Equal(4, configurationInfo.DefaultOrdersCanceledId);
         Assert.Equal(";DIGITAL_PRODUCTS_ACTIVE;KLARNA_CHECKOUT_V3_STATUS", configurationInfo.InstalledApps);
      }

      [Fact]
      public async Task GetStoreStatsAsync_ContactWithLowestAdminId_ShouldBeDefaultMarketingContactAsync()
      {
         var storeName = "teststore_mystore_no";
         var templateName = "america";

         var storeQueryResult = new SingleStoreQueryResult
         {
            StoreDomainInfo = new DomainInfo
            {
               Domain = "Test Domain"
            },
            StoreGeneralStats = new GeneralInfo
            {
               User = storeName,
               DateSignedUp = DateTime.MinValue,
               RevenueLastDay = 100,
               RevenueLast7Days = 300,
               RevenueLast30Days = 500,
               RevenueLast365Days = 750,
               RevenueYearToDate = 750,
            },
            StoreConfigStats = new List<StoreConfiguration>
            {
               new()
               {
                  ConfigurationKey = "TEMPLATE_NAME",
                  ConfigurationValue = templateName
               }
            },
            StoreOrderRevenuePosStats = new OrderRevenuePos
            {
               RevenuePosLastDay = 0,
               RevenuePosLast7Days = 50,
               RevenuePosLast30Days = 100,
               RevenuePosLast365Days = 200,
               RevenuePosYearToDate = 200,
            },
            Contacts = new List<Contact>()
            {
               new Contact()
               {
                  AdminId = 1,
                  FirstName = "Patrick",
                  LastName = "Star",
                  Phone = "1376047",
                  Email = "patrick.star@bikinibottom.com"
               },
               new Contact()
               {
                  AdminId = 2,
                  FirstName = "Sponge Bob",
                  LastName = "Squarepants",
                  Phone = "133769420",
                  Email = "spongebob@bikinibottom.com"
               }
            }
         };
         _storeService.GetStoreStatsAsync(storeName).Returns(storeQueryResult);
         var store = await _sut.GetSingleStoreStatsAsync(storeName);
         var defaultMarketingContact = store.Contacts.Single(contact => contact.AdminId == 1);
         var nonDefaultMarketingContact = store.Contacts.Single(contact => contact.AdminId == 2);
         Assert.True(defaultMarketingContact.DefaultMarketingContact);
         Assert.True(!nonDefaultMarketingContact.DefaultMarketingContact);
      }

      [Fact]
      public void GetStoreConfigByStoreName_ShouldReturnEmpty_WhenNoAppsFound()
      {
         var storeConfiguration = new List<StoreConfiguration>
         {
            new()
            {
               ConfigurationKey = "invalid_key",
               ConfigurationValue = ""
            }
         };
         var configurationInfo = _sut.GetStoreConfigByStoreName(storeConfiguration);
         Assert.True(string.IsNullOrEmpty(configurationInfo.InstalledApps));
      }
   }
}