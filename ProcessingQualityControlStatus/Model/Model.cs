using Microsoft.Xrm.Sdk;
using ProcessingQualityControlStatus.Extension;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingQualityControlStatus.Model
{
    internal struct AppSettings
    {
        internal const string CrmLogin = "";
        internal const string CrmPassword = "";
        internal const string CrmUrlAuth = @"";
        internal const int optionValueHypothec = 962090001; 
        internal static int[] QualifyStatus = new int[] { 962090000, 962090001, 962090002, 962090003, 962090004 };
    }

    internal static class FetchPaging
    {
        // Define the fetch attributes.
        // Set the number of records per page to retrieve.
        internal static int FetchCount = 6000;
        // Initialize the page number.
        internal static int PageNumber = 1;
        // Initialize the number of records.
        internal static int RecordCount = 0;
        // Specify the current paging cookie. For retrieving the first page, 
        // pagingCookie should be null.
        internal static string PagingCookie = null;
    }

    internal static class InitLog
    {
        internal static ILogger logger = new LoggerConfiguration().WriteTo.File($@"{Environment.CurrentDirectory}\Logs\Log_{DateTime.Now.ToString("d")}.txt").CreateLogger();
    }

    internal static class InstanceEntitiesUpdate
    {
        private static List<Entity> entities { get; set; } 
        internal static List<Entity> Entities {
            get {
                if (entities == null)
                {
                    return entities = new List<Entity>();
                }
                return entities;
            }
            set {
                entities = value;
            }
        }
        internal static void PushEntity(IOrganizationService orgSvc)
        {
            IEnumerable<List<Entity>> groupPartitionListing = Entities.Partition(1000);
            foreach (List<Entity> listing in groupPartitionListing)
            {
                try
                {
                    orgSvc.BulkUpdate(listing);
                }
                catch (Exception ex)
                {
                    InitLog.logger.Warning(ex, "Throw exception for bulk updated entity");
                }
            }
        }

        internal static void PushEntityAsync(IOrganizationService orgSvc)
        {
            Task.Run(() =>
            {
                IEnumerable<List<Entity>> groupPartitionListing = Entities.Partition(1000);
                foreach (List<Entity> listing in groupPartitionListing)
                {
                    try
                    {
                        orgSvc.BulkUpdate(listing);
                    }
                    catch (Exception ex)
                    {
                        InitLog.logger.Warning(ex, "Throw exception for bulk updated entity");
                    }
                }
            });
        }
    }
}
