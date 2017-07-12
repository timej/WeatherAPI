using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using System.Linq;

namespace JmaXml.Common
{
    public class JmaDatastore
    {
        private readonly DatastoreDb _db;

        public JmaDatastore(string projectId)
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
            return entity.Properties["update"].TimestampValue.ToDateTime().ToUniversalTime();
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
                    .CreateKey(feedType)),
                    Filter.GreaterThan("created", cutoff)),
                Order = { { "created", PropertyOrder.Types.Direction.Descending } }
            };

            var result = await _db.RunQueryAsync(query);

            return result.Entities;
        }

        //天気予報等のXML用
        public Entity SetEntity(string kind, string task, int office, string forecast, DateTime updateTime)
        {
            Key rootKey = _db.CreateKeyFactory("Task").CreateKey(task);
            Key key = new KeyFactory(rootKey, kind).CreateKey(office);

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

        //天気予報等のXML用
        public async Task<List<(int id, long update)?>> GetJmaUpdateAsync(string task)
        {

            List<(int id, long update)?> updateList = new List<(int id, long update)?>();
            Query query = new Query("JmaXml")
            {
                Filter = Filter.HasAncestor(_db.CreateKeyFactory("Task").CreateKey(task)),
                Projection = { "__key__", "update" }
            };

            var result = await _db.RunQueryAsync(query);
            foreach (var d in result.Entities)
            {
                updateList.Add(((int)d.Key.Path[1].Id, d.Properties["update"].IntegerValue));

            }
            return updateList;
        }

        //Upsert
        public async Task UpsertForecastAsync(List<Entity> entityList)
        {
            await _db.UpsertAsync(entityList);
        }

        //天気予報等のXMLのデータを取得
        public async Task<string> GetJmaXmlAsync(string task, int office)
        {
            Key rootKey = _db.CreateKeyFactory("Task").CreateKey(task);
            Key key = new KeyFactory(rootKey, "JmaXml").CreateKey(office);

            Entity entity = await _db.LookupAsync(key);
            if (entity == null)
                return null;
            return entity.Properties["forecast"].StringValue;
        }


        //天気予報XMLのデータを一括取得
        public async Task<DatastoreQueryResults> GetAllJmaXmlAsync(string kind, string task)
        {
            Query query = new Query(kind)
            {
                Filter = Filter.HasAncestor(_db.CreateKeyFactory("Task").CreateKey(task)),
            };
            return await _db.RunQueryAsync(query);
        }

        //Feedデータの削除
        //hour時間より前のを削除
        public async Task DeleteFeedsAsync(int hour)
        {
            Query query = new Query("JmaFeeds")
            {
                Filter = Filter.LessThan("created", DateTime.UtcNow.AddHours(hour)),
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

