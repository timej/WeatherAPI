using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;

namespace JmaXmlServer.Models
{
    public class Datastore
    {
        private readonly DatastoreDb _db;
        private readonly KeyFactory _keyFactory;

        public Datastore(string project)
        {
            // Create an authorized Datastore service using Application Default Credentials.
            _db = DatastoreDb.Create(AppConst.ProjectId);
            // Create a Key factory to construct keys associated with this project.
            _keyFactory = _db.CreateKeyFactory(project);
        }

        public void AddTask(string feeds)
        {
            Entity task = new Entity()
            {
                Key = _keyFactory.CreateIncompleteKey(),
                ["feeds"] = new Value()
                {
                    StringValue = feeds,
                    ExcludeFromIndexes = true
                },
                ["created"] = DateTime.UtcNow,
            };
            _db.Insert(task);
        }
    }
}
