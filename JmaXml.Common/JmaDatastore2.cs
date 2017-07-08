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

        public async Task<DateTime?> GetUpdateAsync(string kind, string keyname)
        {
            var key = _db.CreateKeyFactory(kind).CreateKey(keyname);
            Entity entity = await _db.LookupAsync(key);
            if (entity == null)
                return null;
            return entity.Properties["update"].TimestampValue.ToDateTime();
        }

        public async Task SetUpdateAsync(string kind, string keyname, DateTime dt)
        {
            Entity entity = new Entity()
            {
                Key = _db.CreateKeyFactory(kind).CreateKey(keyname),
                ["update"] = dt
            };
            await _db.UpsertAsync(entity);
        }

        //公開されるXML電文の更新情報のデータ
        //feedType: regular (定期) extra (臨時)
        public async Task FeedsInsert(string feedType, string feeds, DateTime created)
        {
            Key forcastListKey = _db.CreateKeyFactory("FeedType").CreateKey(feedType);
            Key feedKey = new KeyFactory(forcastListKey, "JmaFeeds").CreateIncompleteKey();
            Entity entity = new Entity()
            {
                Key = feedKey,
                ["feeds"] = new Value()
                {
                    StringValue = feeds,
                    ExcludeFromIndexes = true
                },
                ["created"] = created,
            };
            await _db.InsertAsync(entity);
        }


        //公開されるXML電文の更新情報のデータの取得
        public async Task<IEnumerable<Entity>> GetJmaFeed(string feedType, DateTime cutoff)
        {
            Query query = new Query("JmaFeeds")
            {
                Filter = Filter.And(Filter.HasAncestor(_db.CreateKeyFactory("FeedType")
                    .CreateKey("feedType")),
                    Filter.GreaterThan("created", cutoff)),
                Order = { { "created", PropertyOrder.Types.Direction.Descending } }
            };

            var result = await _db.RunQueryAsync(query);

            return result.Entities;
        }

        public async Task<IEnumerable<Entity>> GetJmaUpdate(string parent, DateTime cutoff)
        {
            Query query = new Query("JmaXml")
            {
                Filter = Filter.HasAncestor(_db.CreateKeyFactory("JmaList").CreateKey(parent)),
                Projection = { "__key__", "update" }
            };

            var result = await _db.RunQueryAsync(query);
            return result.Entities;
        }

        //天気予報等のXML用
        public Entity SetEntity(string root, string second, string kind, int office, string forecast, DateTime updateTime)
        {
            Key rootKey = _db.CreateKeyFactory("JmaList").CreateKey(root);
            Key secondKey = new KeyFactory(rootKey, "Category").CreateKey(second);
            Key key = new KeyFactory(secondKey, kind).CreateKey(office);

            return new Entity()
            {
                Key = key,
                ["forecast"] = new Value()
                {
                    StringValue = forecast,
                    ExcludeFromIndexes = true
                },
                ["update"] = updateTime,
            };
        }

        public async Task UpsertForecastAsync(List<Entity> entityList)
        {
            await _db.UpsertAsync(entityList);
        }
    }
}
