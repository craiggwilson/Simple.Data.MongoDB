using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Simple.Data.MongoDB
{
    internal class MongoAdapterFinder
    {
        private MongoAdapter _mongoAdapter;
        private IExpressionFormatter _expressionFormatter;

        public MongoAdapterFinder(MongoAdapter mongoAdapter, IExpressionFormatter expressionFormatter)
        {
            _mongoAdapter = mongoAdapter;
            _expressionFormatter = expressionFormatter;
        }

        public IEnumerable<IDictionary<string, object>> Find(MongoCollection<BsonDocument> collection, SimpleQuery query)
        {
            if (!query.SkipCount.HasValue && query.TakeCount.HasValue && query.TakeCount.Value == 1)
                return new[] { FindOne(collection, query.Criteria) };

            var cursor = CreateCursor(collection, query.Criteria);

            ApplyFields(cursor, query.Columns);
            ApplySorting(cursor, query.Order);
            ApplySkip(cursor, query.SkipCount);
            ApplyTake(cursor, query.TakeCount);

            var aliases = query.Columns.OfType<ObjectReference>().ToDictionary(x => x.GetName(), x => x.Alias);

            return cursor.Select(x => x.ToDictionary(aliases));
        }

        public IDictionary<string, object> FindOne(MongoCollection<BsonDocument> collection, SimpleExpression criteria)
        {
            if (criteria == null)
                return collection.FindOne().ToDictionary();

            var mongoQuery = _expressionFormatter.Format(criteria);
            var results = collection.FindOne(mongoQuery);

            return results.ToDictionary();
        }

        private void ApplyFields(MongoCursor<BsonDocument> cursor, IEnumerable<SimpleReference> columns)
        {
            if (!columns.Any())
                return;

            var fields = columns.Select(x => string.Join(".", x.ToString().Split('.').Skip(1)))
                .Select(x => (x == "Id" || x == "id") ? "_id" : x);

            cursor.SetFields(fields.ToArray());
        }

        private void ApplySorting(MongoCursor<BsonDocument> cursor, IEnumerable<SimpleOrderByItem> orderings)
        {
            if (orderings == null || !orderings.Any())
                return;

            var sortBuilder = new SortByBuilder();
            foreach (var ordering in orderings)
            {
                var name = ExpressionFormatter.GetFullName(ordering.Reference);
                if (ordering.Direction == OrderByDirection.Ascending)
                    sortBuilder.Ascending(name);
                else
                    sortBuilder.Descending(name);
            }

            cursor.SetSortOrder(sortBuilder);
        }

        private void ApplySkip(MongoCursor<BsonDocument> cursor, int? skipCount)
        {
            if (skipCount.HasValue)
                cursor.SetSkip(skipCount.Value);
        }

        private void ApplyTake(MongoCursor<BsonDocument> cursor, int? takeCount)
        {
            if (takeCount.HasValue)
                cursor.SetLimit(takeCount.Value);
        }

        private MongoCursor<BsonDocument> CreateCursor(MongoCollection<BsonDocument> collection, SimpleExpression criteria)
        {
            if (criteria == null)
                return collection.FindAll();

            var mongoQuery = _expressionFormatter.Format(criteria);
            return collection.Find(mongoQuery);
        }
    }
}