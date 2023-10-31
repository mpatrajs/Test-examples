using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Azure.WebJobs;
using Mystore.Functions.Interfaces;
using Mystore.Functions.Models;

namespace Mystore.Functions.Helpers
{
   public class StoreService : IStoreService
   {
      private readonly IStoreRepository _storeRepository;

      public StoreService(IStoreRepository storeRepository) => _storeRepository = storeRepository;

      public async Task<IEnumerable<string>> GetAllStoresAsync()
      {
         var allStores = await _storeRepository.GetAllStoresAsync();
         return allStores.Select(x => x.User);
      }

      public async Task<IEnumerable<dynamic>> ExecuteCustomQuery(string query)
      {
         var result = await _storeRepository.ExecuteCustomQueryAsync(query);
         return result;
      }


      public async Task<Store> GetSingleStoreStatsAsync(string name)
      {
         var storeStats = await _storeRepository.GetStoreStatsAsync(name);
         var configStats = GetStoreConfigByStoreName(storeStats.StoreConfigStats);
         var generalInfo = SubtractPosRevenueFromEstore(storeStats.StoreGeneralStats, storeStats.StoreOrderRevenuePosStats);

         if (storeStats.StoreGeneralStats.DateSignedUp == DateTime.MinValue)
         {
            storeStats.StoreGeneralStats.DateSignedUp = null;
         }

         var storeInfo = new Store
         {
            Id = name,
            ObjectType = "store",
            General = generalInfo,
            Domains = storeStats.StoreDomainInfo.Domain,
            InvoiceInfo = storeStats.StoreOrderInvoiceStats,
            ModuleOrderTotalCount = storeStats.StoreOrderCountStats,
            ModuleOrderLastDay = storeStats.StoreOrderLastDayStats,
            ModuleOrderLast7Days = storeStats.StoreOrderLast7DaysStats,
            ModuleOrderLast30Days = storeStats.StoreOrderLast30DaysStats,
            ModuleOrderLast3Months = storeStats.StoreOrderLast3MonthsStats,
            ModuleOrderLast6Months = storeStats.StoreOrderLast6MonthsStats,
            ModuleOrderLast9Months = storeStats.StoreOrderLast9MonthsStats,
            ModuleOrderLast365Days = storeStats.StoreOrderLast365DaysStats,
            ModuleOrderYearToDate = storeStats.StoreOrderYearToDateStats,
            OrderAverageLinesPerOrder = storeStats.StoreOrderAverageLinesPerOrderStats,
            OrderRevenuePos = storeStats.StoreOrderRevenuePosStats,
            CreditHistory = storeStats.StoreCreditStats,
            Products = storeStats.StoreProductStats,
            Contacts = ProcessContacts(storeStats.Contacts),
            Configuration = configStats
         };

         return storeInfo;
      }

      public async Task StoreSingleStoreStatsAsync(Store storeInfo, IAsyncCollector<dynamic> documentsOut) => await documentsOut.AddAsync(storeInfo);

      public ConfigurationInfo GetStoreConfigByStoreName(IEnumerable<StoreConfiguration> configStats)
      {
         var installedApps = "";

         var storeConfigurations = configStats.ToList();
         if (!storeConfigurations.Any())
         {
            return null;
         }

         foreach (var app in MystoreApps.FriendlyNames.Where(app => storeConfigurations.Any(x => string.Equals(x.ConfigurationKey, app.Value, StringComparison.CurrentCultureIgnoreCase))))
         {
            installedApps = $"{installedApps};{app.Value}";
         }

         return new ConfigurationInfo
         {
            InstalledApps = installedApps,
            CrallSearch = storeConfigurations.Any(x => x.ConfigurationKey == "CRALL_SEARCH"),
            CrallStatus = storeConfigurations.Any(x => x.ConfigurationKey == "CRALL_STATUS"),
            CrallRecommendation = storeConfigurations.Any(x => x.ConfigurationKey == "CRALL_RECOMMENDATION"),
            DefaultOrdersCanceledId = Convert.ToInt32(storeConfigurations.FirstOrDefault(x => x.ConfigurationKey == "DEFAULT_ORDERS_CANCELED_ID")?.ConfigurationValue),
            Templates = storeConfigurations.Any(x => x.ConfigurationKey == "TEMPLATE_NAME") ? storeConfigurations.FirstOrDefault(x => x.ConfigurationKey == "TEMPLATE_NAME").ConfigurationValue : null,
            KlarnaInstalled = storeConfigurations.Any(x => x.ConfigurationKey == "KLARNA_CHECKOUT_ENABLED" || x.ConfigurationKey == "KLARNA_CHECKOUT_V3_STATUS" || x.ConfigurationKey == "KLARNA_NATIVE_STATUS")
         };
      }

      public GeneralInfo SubtractPosRevenueFromEstore(GeneralInfo generalInfo, OrderRevenuePos revenuePos)
      {
         if (revenuePos != null)
         {
            generalInfo.RevenueLastDay -= revenuePos.RevenuePosLastDay;
            generalInfo.RevenueLast7Days -= revenuePos.RevenuePosLast7Days;
            generalInfo.RevenueLast30Days -= revenuePos.RevenuePosLast30Days;
            generalInfo.RevenueLast365Days -= revenuePos.RevenuePosLast365Days;
            generalInfo.RevenueLast3Months -= revenuePos.RevenuePosLast3Months;
            generalInfo.RevenueLast6Months -= revenuePos.RevenuePosLast6Months;
            generalInfo.RevenueLast9Months -= revenuePos.RevenuePosLast9Months;
            generalInfo.RevenueYearToDate -= revenuePos.RevenuePosYearToDate;
            generalInfo.RevenueTotal -= revenuePos.RevenuePosTotal;
         }

         return generalInfo;
      }

      private static List<Contact> ProcessContacts(IEnumerable<Contact> contacts)
      {
         var validContacts = new List<Contact>();
         var validator = new ContactValidator();
         foreach (var contact in contacts)
         {
            var validationResult = validator.Validate(contact);
            if (validationResult.IsValid)
            {
               validContacts.Add(contact);
            }
         }
         if (!validContacts.Any())
         {
            return validContacts;
         }
         var minAdminId = validContacts.Min(x => x.AdminId);
         foreach (var contact in validContacts)
         {
            if (contact.AdminId == minAdminId)
            {
               contact.DefaultMarketingContact = true;
            }
         }
         return validContacts;
      }
   }
}