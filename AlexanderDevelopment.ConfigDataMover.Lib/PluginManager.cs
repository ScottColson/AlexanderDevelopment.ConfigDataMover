using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using log4net;


namespace AlexanderDevelopment.ConfigDataMover.Lib
{
    using ProxyClasses;

    public static class PluginSDKStepManager 
    {
        public static void DisableSdkMessageSteps(List<sdkmessageprocessingstep> messageSteps, IOrganizationService orgService, ILog logger)
        {
            foreach(var step in messageSteps)
            {
                logger.Info(string.Format("Disabling SDK Message step {0}", step.Id));
                step.SetState(orgService, sdkmessageprocessingstep.eStatus.Disabled, sdkmessageprocessingstep.eStatusReason.Disabled_Disabled);
            }
        }

        public static void EnableSdkMessageSteps(List<sdkmessageprocessingstep> messageSteps, IOrganizationService orgService, ILog logger)
        {
            foreach (var step in messageSteps)
            {
                logger.Info(string.Format("Enabling SDK Message step {0}", step.Id));
                step.SetState(orgService, sdkmessageprocessingstep.eStatus.Enabled, sdkmessageprocessingstep.eStatusReason.Enabled_Enabled);
            }
        }

        /// <summary>
        /// Returns a list of SDK Message Step referneces related to the specified entity that are currently active.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="orgService"></param>
        /// <returns></returns>
        public static List<sdkmessageprocessingstep> GetEnableSdkMessageSteps(string entityLogicalName, IOrganizationService orgService, ILog logger)
        {
            //Get object type code from the entity logical name.   
            logger.Info(string.Format("Retrieving SDK Message steps for entity {0}", entityLogicalName));
            var resp = (RetrieveEntityResponse)orgService.Execute(
                new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    LogicalName = entityLogicalName
                });
            var typeCode = resp.EntityMetadata.ObjectTypeCode;

            //query for all enabled SDK message steps related to the type code.
            var qry = new QueryExpression
            {
                EntityName = sdkmessageprocessingstep.LogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = sdkmessageprocessingstep.Properties.statecode,
                            Operator = ConditionOperator.Equal,
                            Values = {(int)sdkmessageprocessingstep.eStatus.Enabled}
                        }
                        
                    },
                    Filters =
                    {
                        new FilterExpression
                        {
                            FilterOperator = LogicalOperator.Or,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = sdkmessageprocessingstep.Properties.stage,
                                    Operator = ConditionOperator.Equal,
                                    Values = {10}
                                },
                                 new ConditionExpression
                                {
                                    AttributeName = sdkmessageprocessingstep.Properties.stage,
                                    Operator = ConditionOperator.Equal,
                                    Values = {20}
                                },
                                  new ConditionExpression
                                {
                                    AttributeName = sdkmessageprocessingstep.Properties.stage,
                                    Operator = ConditionOperator.Equal,
                                    Values = {40}
                                },
                                   new ConditionExpression
                                {
                                    AttributeName = sdkmessageprocessingstep.Properties.stage,
                                    Operator = ConditionOperator.Equal,
                                    Values = {50}
                                }
                            }
                        }
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        JoinOperator = JoinOperator.Inner,
                        LinkFromEntityName = sdkmessageprocessingstep.LogicalName,
                        LinkFromAttributeName = sdkmessageprocessingstep.Properties.sdkmessagefilterid,
                        LinkToEntityName = sdkmessagefilter.LogicalName,
                        LinkToAttributeName = sdkmessagefilter.Properties.sdkmessagefilterid,
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.Or,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = sdkmessagefilter.Properties.primaryobjecttypecode,
                                    Operator = ConditionOperator.Equal,
                                    Values = {typeCode }
                                },
                                new ConditionExpression
                                {
                                    AttributeName = sdkmessagefilter.Properties.secondaryobjecttypecode,
                                    Operator = ConditionOperator.Equal,
                                    Values = {typeCode}
                                }
                            }
                        }
                    }
                }

            };

            return orgService.RetrieveMultiple(qry).ToProxies<sdkmessageprocessingstep>();
            
        }
    }
}
