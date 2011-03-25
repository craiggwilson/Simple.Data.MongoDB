using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Dynamic;
using MongoDB.Driver.Builders;

namespace Simple.Data.MongoDB
{
    internal class MongoAdapterUpdater
    {
        private readonly MongoAdapter _adapter;
        private readonly IExpressionFormatter _expressionFormatter;

        public MongoAdapterUpdater(MongoAdapter adapter, IExpressionFormatter expressionFormatter)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;
            _expressionFormatter = expressionFormatter;
        }

        public int Update(MongoCollection<BsonDocument> collection, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var condition = _expressionFormatter.Format(criteria);

            if (data.ContainsKey("Id"))
            {
                data["_id"] = data["Id"];
                data.Remove("Id");
            }

            var update = new UpdateDocument("$set", data.ToBsonDocument());

            var result = collection.Update(condition, update, UpdateFlags.Multi);
            if (result != null)
                return result.DocumentsAffected;

            return int.MaxValue;
        }
    }
}