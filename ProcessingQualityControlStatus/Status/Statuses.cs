using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ProcessingQualityControlStatus.Helper;
using ProcessingQualityControlStatus.Model;
using System;

namespace ProcessingQualityControlStatus.Status
{
    public class HelperStatus
    {
        public static void PreparingStatus(IOrganizationService orgSvc, Entity entity, DateTime? signingDateFact, DateTime? paymentDate, DateTime? activateDate, OptionSetValue paymentOption, EntityReference mainOpportunity, DateTime? estimatedCloseDate)
        {
            if (CheckSigningDate(signingDateFact, paymentDate))
            {
                HelperMethods.AddInstanceForUpdate(entity, 1);
            }

            if (CheckAct(signingDateFact, paymentDate, activateDate))
            {
                HelperMethods.AddInstanceForUpdate(entity, 2);
            }

            if (CheckEstimatedCloseDate(paymentOption, mainOpportunity, estimatedCloseDate))
            {
                HelperMethods.AddInstanceForUpdate(entity, 3);
            }

            if (CheckEstimatedEscrowUnclosedDate(paymentOption, mainOpportunity, estimatedCloseDate, orgSvc))
            {
                HelperMethods.AddInstanceForUpdate(entity, 3);
            }

            if (CheckDelayDate(mainOpportunity, estimatedCloseDate))
            {
                HelperMethods.AddInstanceForUpdate(entity, 4);
            }

            if (CheckDelayEscrowDate(mainOpportunity, estimatedCloseDate, orgSvc))
            {
                HelperMethods.AddInstanceForUpdate(entity, 4);
            }
        }

        public static bool CheckSigningDate(DateTime? signingDateFact, DateTime? paymentDate)
        {
            return !signingDateFact.HasValue && paymentDate.HasValue;
        }

        public static bool CheckAct(DateTime? signingDateFact, DateTime? paymentDate, DateTime? activateDate)
        {
            return signingDateFact.HasValue && paymentDate.HasValue && paymentDate.Value.AddDays(30) <= DateTime.Now && !activateDate.HasValue;
        }

        public static bool CheckEstimatedCloseDate(OptionSetValue paymentOption, EntityReference mainOpportunity, DateTime? estimatedCloseDate)
        {
            if (paymentOption?.Value == AppSettings.optionValueHypothec && mainOpportunity == null && estimatedCloseDate.HasValue)
            {
                TimeSpan calculateRemainder = estimatedCloseDate.Value - DateTime.Now;
                if (calculateRemainder.Days >= 2 && calculateRemainder.Days <= 4)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckEstimatedEscrowUnclosedDate(OptionSetValue paymentOption, EntityReference mainOpportunity, DateTime? estimatedCloseDate, IOrganizationService service)
        {
            if (paymentOption?.Value == AppSettings.optionValueHypothec && estimatedCloseDate.HasValue)
            {
                TimeSpan calculateRemainder = estimatedCloseDate.Value - DateTime.Now;
                if (calculateRemainder.Days >= 2 && calculateRemainder.Days <= 4 && mainOpportunity != null && !EscrowDateHasValue(service, mainOpportunity))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckDelayDate(EntityReference mainOpportunity, DateTime? estimatedCloseDate)
        {
            if (mainOpportunity == null && estimatedCloseDate.HasValue)
            {
                TimeSpan calculateRemainder = estimatedCloseDate.Value - DateTime.Now;
                if (calculateRemainder.Days <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckDelayEscrowDate(EntityReference mainOpportunity, DateTime? estimatedCloseDate, IOrganizationService service)
        {
            if (mainOpportunity != null && estimatedCloseDate.HasValue)
            {
                TimeSpan calculateRemainder = estimatedCloseDate.Value - DateTime.Now;
                if (calculateRemainder.Days <= 1 && mainOpportunity != null && !EscrowDateHasValue(service, mainOpportunity))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool EscrowDateHasValue(IOrganizationService service, EntityReference mainOpportunity)
        {
            Entity opportunity = service.Retrieve("opportunity", mainOpportunity.Id, new ColumnSet("mtr_escrowdate"));
            return opportunity.GetAttributeValue<DateTime?>("mtr_escrowdate").HasValue;
        }
    }
}
