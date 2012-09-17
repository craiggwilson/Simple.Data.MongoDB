using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Dynamic;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Simple.Data.MongoDB
{
    internal class MongoAdapterInserter
    {
        private readonly MongoAdapter _adapter;

        public MongoAdapterInserter(MongoAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;
        }

        public IDictionary<string, object> Insert(MongoCollection<BsonDocument> collection, IDictionary<string, object> data)
        {
            MongoIdKeys.ReplaceId(data);

            var doc = ConvertToDocument(data);
            collection.Insert(doc);
            return doc.ToDictionary();
        }

        private BsonDocument ConvertToDocument(IDictionary<string, object> data)
        {
            var doc = new BsonDocument();
            using (var bsonWriter = BsonWriter.Create(doc))
            {
                if (data.Count == 0)
                {
                    bsonWriter.WriteNull();
                    return doc;
                }

                bsonWriter.WriteStartDocument();
                foreach (var pair in data)
                {
                    bsonWriter.WriteName(pair.Key);
                    var memberValue = pair.Value;
                    if (memberValue == null)
                        bsonWriter.WriteNull();
                    else
                    {
                        var memberType = memberValue.GetType();
                        var serializer = BsonSerializer.LookupSerializer(memberType);
                        serializer.Serialize(bsonWriter, memberType, memberValue, null);
                    }
                }
                bsonWriter.WriteEndDocument();
            }
            return doc;
        }
    }
}