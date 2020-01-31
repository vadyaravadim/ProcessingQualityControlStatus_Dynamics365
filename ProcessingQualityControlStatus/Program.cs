using DataLoaderTools;
using DataLoaderTools.Connector;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Serilog;
using System.Reflection;
using ProcessingQualityControlStatus.Request;
using ProcessingQualityControlStatus.Status;
using ProcessingQualityControlStatus.Model;
using ProcessingQualityControlStatus.Extension;
using ProcessingQualityControlStatus.Helper;

namespace ProcessingQualityControlStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            InitLog.logger.Information("started application for update qualify status");
            IOrganizationService orgSvc = HelperRequest.ConnectionCrm();
            List<Entity[]> reservationContracts = HelperRequest.GetPagingReservationContract(orgSvc);
            ProcessingStatus(orgSvc, reservationContracts);
            InitLog.logger.Information("successful entity update");
        }

        private static void ProcessingStatus(IOrganizationService orgSvc, List<Entity[]> reservationContracts)
        {
            foreach (Entity[] entities in reservationContracts)
            {
                Parallel.ForEach(entities, (entity) =>
                {
                    DateTime? signingDateFact = entity.GetAttributeValue<DateTime?>("mtr_signing_date_fact"); // Дата подписания (факт)
                    DateTime? paymentDate = entity.GetAttributeValue<DateTime?>("mtr_payment_date"); // Дата первого платежа
                    DateTime? activateDate = entity.GetAttributeValue<DateTime?>("mtr_actdate1"); // Дата актирования 1
                    OptionSetValue paymentOption = entity.GetAttributeValue<OptionSetValue>("mtr_payment_option");  // Вариант оплаты
                    EntityReference mainOpportunity = entity.GetAttributeValue<EntityReference>("mtr_main_opportunityid"); // Договор продажи
                    DateTime? estimatedCloseDate = entity.GetAttributeValue<DateTime?>("mtr_estimatedclosedate"); // Срок бронирования

                    HelperStatus.PreparingStatus(orgSvc, entity, signingDateFact, paymentDate, activateDate, paymentOption, mainOpportunity, estimatedCloseDate);
                });
            }

            InitLog.logger.Information($"quantity entities for update {InstanceEntitiesUpdate.Entities.Count}");
            InstanceEntitiesUpdate.PushEntityAsync(orgSvc);
        }
    }
}
