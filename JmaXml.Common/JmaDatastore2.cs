using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;


namespace JmaXml.Common
{
    public class JmaDatastore2
    {
        private readonly DatastoreDb _db;

        public JmaDatastore2(string projectId)
        {
            // Create an authorized Datastore service using Application Default Credentials.
            _db = DatastoreDb.Create(projectId);
        }

        //feedType: regular (定期) extra (臨時)
        public async Task FeedsInsert(string feedType, string feeds, DateTime created)
        {
            Key forcastListKey = _db.CreateKeyFactory("JmaForcastList").CreateKey(feedType);
            Key feedKey = new KeyFactory(forcastListKey, "Feeds").CreateIncompleteKey();
            Entity entity = new Entity()
            {
                ["feeds"] = new Value()
                {
                    StringValue = feeds,
                    ExcludeFromIndexes = true
                },
                ["created"] = created,
            };
            await _db.InsertAsync(entity);
        }
    }
}
