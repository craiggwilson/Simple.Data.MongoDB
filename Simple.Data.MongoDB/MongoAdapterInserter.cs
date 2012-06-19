using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Dynamic;

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

            var doc = data.ToBsonDocument();
            collection.Insert(doc);
            return doc.ToDictionary();
        }
    }
}