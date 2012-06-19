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

        public IEnumerable<IDictionary<string, object>> Find(MongoCollection<BsonDocument> collection, SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = MongoQueryBuilder.BuildFrom(query);

            unhandledClauses = builder.UnprocessedClauses;

            if (builder.IsTotalCountQuery)
            {
                long count;
                if (builder.Criteria == null)
                    count = collection.Count();
                else
                    count = collection.Count(_expressionFormatter.Format(builder.Criteria));

                //TODO: figure out how to make count a long
                builder.SetTotalCount((int)count);
            }

            if (!builder.SkipCount.HasValue && builder.TakeCount.HasValue && builder.TakeCount.Value == 1)
                return new[] { FindOne(collection, builder.Criteria) };

            var cursor = CreateCursor(collection, builder.Criteria);

            ApplyFields(cursor, builder.Columns);
            ApplySorting(cursor, builder.Order);
            ApplySkip(cursor, builder.SkipCount);
            ApplyTake(cursor, builder.TakeCount);

            var aliases = builder.Columns.OfType<ObjectReference>().ToDictionary(x => ExpressionFormatter.GetFullName(x), x => x.GetAlias());

            return cursor.Select(x => x.ToSimpleDictionary(aliases));
        }

        public IEnumerable<IDictionary<string, object>> Find(MongoCollection<BsonDocument> collection, SimpleExpression criteria)
        {
            if (criteria == null)
                return collection.FindAll().Select(x => x.ToSimpleDictionary());

            var mongoQuery = _expressionFormatter.Format(criteria);
            var results = collection.Find(mongoQuery);

            return results.Select(x => x.ToSimpleDictionary());
        }

        public IDictionary<string, object> FindOne(MongoCollection<BsonDocument> collection, SimpleExpression criteria)
        {
            if (criteria == null)
                return collection.FindOne().ToSimpleDictionary();

            var mongoQuery = _expressionFormatter.Format(criteria);
            var results = collection.FindOne(mongoQuery);

            return results.ToSimpleDictionary();
        }

        private void ApplyFields(MongoCursor<BsonDocument> cursor, IEnumerable<SimpleReference> columns)
        {
            if (columns == null || !columns.Any())
                return;

            var fields = columns.Select(x => string.Join(".", x.ToString().Split('.').Skip(1)))
                .Select(x => MongoIdKeys.Comparer.Equals(x, "id") ? "_id" : x);

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