using DataLoaderTools;
using DataLoaderTools.Connector;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using ProcessingQualityControlStatus.Model;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using ProcessingQualityControlStatus.Helper;

namespace ProcessingQualityControlStatus.Request
{
    public class HelperRequest
    {
        public static IOrganizationService ConnectionCrm()
        {
            ConnectorFactory connector = new ConnectorFactory();
            ConnectionData connection = new ConnectionData
            {
                Login = AppSettings.CrmLogin,
                Password = AppSettings.CrmPassword,
                Url = AppSettings.CrmUrlAuth
            };
            IOrganizationService orgSvc = connector.Create(connection, ConnectorFactory.Developer.Metrium);
            return orgSvc;
        }

        internal static List<Entity[]> GetPagingReservationContract(IOrganizationService orgSvc)
        {
            string fetchToReservationContract = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                  <entity name='mtr_reservation_contract'>
                                                    <attribute name='mtr_reservation_contractid' />
                                                    <attribute name='mtr_name' />
                                                    <attribute name='mtr_signing_date_fact' />
                                                    <attribute name='mtr_payment_date' />
                                                    <attribute name='mtr_actdate1' />
                                                    <attribute name='mtr_payment_option' />
                                                    <attribute name='mtr_main_opportunityid' />
                                                    <attribute name='mtr_estimatedclosedate' />
                                                    <order attribute='mtr_name' descending='false' />
                                                    <filter type='and'>
                                                      <condition attribute='statecode' operator='eq' value='0' />
                                                      <condition attribute='mtr_contract_type' operator='ne' value='962090010' />
                                                    </filter>
                                                  </entity>
                                                </fetch>";
            EntityCollection response;
            string pagingXml;
            List<Entity[]> pagingReservationContract = new List<Entity[]>();

            do
            {
                pagingXml = HelperMethods.CreateXml(fetchToReservationContract, FetchPaging.PagingCookie, FetchPaging.PageNumber, FetchPaging.FetchCount);
                response = orgSvc.RetrieveMultiple(new FetchExpression(pagingXml));

                if (response.MoreRecords)
                {
                    FetchPaging.PageNumber++;
                    FetchPaging.PagingCookie = response.PagingCookie;
                }

                Entity[] reservationContracts = response.Entities.ToArray();
                pagingReservationContract.Add(reservationContracts);
                InitLog.logger.Information($"quantity entities for processing {reservationContracts.Length}");

            } while (response.MoreRecords);

            return pagingReservationContract;
        }
    }
}
