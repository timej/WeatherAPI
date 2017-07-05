using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;

namespace JmaXml.Common
{
    public class JmaDatastore
    {
        private readonly DatastoreDb _db;
        private readonly KeyFactory _keyFactory;

        public JmaDatastore(string projectId, string kind)
        {
            // Create an authorized Datastore service using Application Default Credentials.
            _db = DatastoreDb.Create(projectId);
            // Create a Key factory to construct keys associated with this project.
            _keyFactory = _db.CreateKeyFactory(kind);
        }

        public async Task AddTask(string feeds, DateTime created)
        {
            Entity task = new Entity()
            {
                Key = _keyFactory.CreateIncompleteKey(),
                ["feeds"] = new Value()
                {
                    StringValue = feeds,
                    ExcludeFromIndexes = true
                },
                ["created"] = created,
            };
            await _db.InsertAsync(task);
        }

        //
        public async Task<IEnumerable<Entity>> GetJmaFeed(string kind, DateTime cutoff)
        {

            Query query = new Query(kind)
            {
                Filter = Filter.GreaterThan("created", cutoff),
                Order = { { "created", PropertyOrder.Types.Direction.Descending } }
            };
            var result = await _db.RunQueryAsync(query);
            return result.Entities;
        }

        public async Task<DateTime?> GetUpdateAsync(string keyname) 
        {
            var key = _keyFactory.CreateKey(keyname);
            Entity entity = await _db.LookupAsync(key);
            if (entity == null)
                return null;
            return entity.Properties["update"].TimestampValue.ToDateTime();
        }

        public async Task SetUpdateAsync(string keyname, DateTime dt)
        {
            Entity entity = new Entity()
            {
                Key = _keyFactory.CreateKey(keyname),
                ["update"] = dt
            };
            await _db.UpsertAsync(entity);
        }

        //天気予報等のXML用
        public Entity SetEntity(int office, string forecast, DateTime updateTime)
        {
            return new Entity()
            {
                Key = _keyFactory.CreateKey(office),
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

        //天気予報等のXMLのデータを取得
        public async Task<string> GetJmaXmlAsync(int office)
        {
            var key = _keyFactory.CreateKey(office);
            Entity entity = await _db.LookupAsync(key);
            if (entity == null)
                return null;
            return entity.Properties["forecast"].StringValue;
        }

        //天気予報XMLのデータを一括取得
        public async Task<DatastoreQueryResults> GetAllJmaXmlAsync(string kind)
        {
            Query query = new Query(kind);
            return await _db.RunQueryAsync(query);
        }

        //Feedデータの削除
        public async Task DeleteFeedsAsync(string kind)
        {
            Query query = new Query(kind)
            {
                Filter = Filter.LessThan("created", DateTime.UtcNow.AddDays(-1)),
                Projection = { "__key__" },
                Limit = 500
            };

            while (true)
            {
                var entries = await _db.RunQueryAsync(query);
                if (!entries.Entities.Any())
                    break;
                await _db.DeleteAsync(entries.Entities);
                await Task.Delay(1000);
            }
        }
    }
}
