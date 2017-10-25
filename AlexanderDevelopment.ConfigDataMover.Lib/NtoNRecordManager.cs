using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using log4net;

namespace AlexanderDevelopment.ConfigDataMover.Lib
{
    public static class NtoNRecordManager
    {
        /// <summary>
        /// Create new N:N associations after purging all existing associations.
        /// </summary>
        /// <param name="records"></param>
        /// <param name="orgService"></param>
        public static void CreateRelationships(List<Entity> records, IOrganizationService orgService, ILog logger)
        {
            if(records.Count > 0)
            {
                var metaData = getRelationshipMetaDataFromRecord(records[0], orgService);

                purgeExistingRelationships(metaData, orgService, logger);

                logger.Info(string.Format("Adding {0} relationsips.", records.Count));

                foreach (var record in records)
                {
                    var entity1Id = new EntityReference(metaData.Entity1LogicalName, record.GetAttributeValue<Guid>(metaData.Entity1IntersectAttribute));
                    var entity2Id = new EntityReference(metaData.Entity2LogicalName, record.GetAttributeValue<Guid>(metaData.Entity2IntersectAttribute));

                    var req = new AssociateRequest
                    {
                        Relationship = new Relationship(metaData.SchemaName),
                        Target = entity1Id,
                        RelatedEntities = new EntityReferenceCollection { entity2Id }
                    };

                    if(entity1Id.LogicalName == entity2Id.LogicalName)
                    {
                        req.Relationship.PrimaryEntityRole = EntityRole.Referenced;
                    }

                    logger.Info(string.Format("Adding relationship between {0}-{1} and {2}-{3}.", entity1Id.LogicalName, entity1Id.Id, entity2Id.LogicalName, entity2Id.Id));

                    orgService.Execute(req);
                }

            }
        }

        
        public static void purgeExistingRelationships(ManyToManyRelationshipMetadata metaData , IOrganizationService orgService, ILog logger)
        {
            logger.Info(string.Format("Purging existing record associations for relationship {0}", metaData.SchemaName));
            //use metadata to create a qry to retreive all records in the N:N table.
            var qry = new QueryExpression
            {
                EntityName = metaData.IntersectEntityName,
                ColumnSet = new ColumnSet(
                    metaData.Entity1IntersectAttribute,
                    metaData.Entity2IntersectAttribute)
            };
          

            var existingRelationships = orgService.RetrieveMultiple(qry).Entities;

            logger.Info(string.Format("Found {0} existing relationships", existingRelationships.Count));

            //remove each existing association
            foreach(var record in existingRelationships)
            {
                var entity1Id = new EntityReference(metaData.Entity1LogicalName, record.GetAttributeValue<Guid>(metaData.Entity1IntersectAttribute));
                var entity2Id = new EntityReference(metaData.Entity2LogicalName, record.GetAttributeValue<Guid>(metaData.Entity2IntersectAttribute));

                var req = new DisassociateRequest
                {
                    Relationship = new Relationship(metaData.SchemaName),
                    Target = entity1Id,
                    RelatedEntities = new EntityReferenceCollection { entity2Id }
                };

                if (entity1Id.LogicalName == entity2Id.LogicalName)
                {
                    req.Relationship.PrimaryEntityRole = EntityRole.Referenced;
                }

                logger.Info(string.Format("Removing link between {0}-{1} and {2}-{3}", entity1Id.LogicalName, entity1Id.Id, entity2Id.LogicalName, entity2Id.Id));
                orgService.Execute(req);

                req = new DisassociateRequest
                {
                    Relationship = new Relationship(metaData.SchemaName),
                    Target = entity2Id,
                    RelatedEntities = new EntityReferenceCollection { entity1Id }
                };

                if (entity1Id.LogicalName == entity2Id.LogicalName)
                {
                    req.Relationship.PrimaryEntityRole = EntityRole.Referenced;
                }

                logger.Info(string.Format("Removing link between {2}-{3} and {0}-{1}", entity1Id.LogicalName, entity1Id.Id, entity2Id.LogicalName, entity2Id.Id));
                orgService.Execute(req);
            }
        }
       
        private static ManyToManyRelationshipMetadata getRelationshipMetaDataFromRecord(Entity record, IOrganizationService orgService)
        {
            var req = new RetrieveRelationshipRequest { Name = record.LogicalName };
            var resp = (RetrieveRelationshipResponse)orgService.Execute(req);

            return (ManyToManyRelationshipMetadata)resp.RelationshipMetadata;
        }

        
    }
}
