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
            var queryCriteria = new QueryCriteria();
            unhandledClauses = queryCriteria.Apply(query.Clauses);

            if (!queryCriteria.SkipCount.HasValue && queryCriteria.TakeCount.HasValue && queryCriteria.TakeCount.Value == 1)
                return new[] { FindOne(collection, queryCriteria.Criteria) };

            var cursor = CreateCursor(collection, queryCriteria.Criteria);

            ApplyFields(cursor, queryCriteria.Columns);
            ApplySorting(cursor, queryCriteria.Order);
            ApplySkip(cursor, queryCriteria.SkipCount);
            ApplyTake(cursor, queryCriteria.TakeCount);

            var aliases = queryCriteria.Columns.OfType<ObjectReference>().ToDictionary(x => ExpressionFormatter.GetFullName(x), x => x.Alias);

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
            if (columns == null || !columns.Any())
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

        private class QueryCriteria
        {
            public IEnumerable<SimpleReference> Columns { get; private set; }

            public SimpleExpression Criteria { get; private set; }

            public IEnumerable<SimpleOrderByItem> Order { get; private set; }

            public int? SkipCount { get; private set; }

            public int? TakeCount { get; private set; }

            public QueryCriteria()
            {
                Columns = Enumerable.Empty<SimpleReference>();
                Order = Enumerable.Empty<SimpleOrderByItem>();
            }

            public IEnumerable<SimpleQueryClauseBase> Apply(IEnumerable<SimpleQueryClauseBase> clauses)
            {
                clauses = ApplyOrderClauses(clauses);
                clauses = ApplySelectClauses(clauses);
                clauses = ApplySkipClauses(clauses);
                clauses = ApplyTakeClauses(clauses);
                clauses = ApplyWhereClauses(clauses);
                return clauses;
            }

            private IEnumerable<SimpleQueryClauseBase> ApplyOrderClauses(IEnumerable<SimpleQueryClauseBase> clauses)
            {
                var list = clauses.OfType<OrderByClause>().ToList();

                var order = new List<SimpleOrderByItem>();
                foreach (var clause in list)
                    order.Add(new SimpleOrderByItem(clause.Reference, clause.Direction));

                Order = order;

                return clauses.Where(x => !list.Contains(x));
            }

            private IEnumerable<SimpleQueryClauseBase> ApplySelectClauses(IEnumerable<SimpleQueryClauseBase> clauses)
            {
                var list = clauses.OfType<SelectClause>().ToList();

                Columns = list.SelectMany(x => x.Columns);

                return clauses.Where(x => !list.Contains(x));
            }

            private IEnumerable<SimpleQueryClauseBase> ApplySkipClauses(IEnumerable<SimpleQueryClauseBase> clauses)
            {
                var list = clauses.OfType<SkipClause>().ToList();

                if (list.Count == 1)
                {
                    SkipCount = list[0].Count;
                }
                else if (list.Count > 1)
                {
                    throw new NotSupportedException("MongoDB does not support multiple skip counts.");
                }

                return clauses.Where(x => !list.Contains(x));
            }

            private IEnumerable<SimpleQueryClauseBase> ApplyTakeClauses(IEnumerable<SimpleQueryClauseBase> clauses)
            {
                var list = clauses.OfType<TakeClause>().ToList();

                if (list.Count == 1)
                {
                    TakeCount = list[0].Count;
                }
                else if (list.Count > 1)
                {
                    throw new NotSupportedException("MongoDB does not support multiple take counts.");
                }

                return clauses.Where(x => !list.Contains(x));
            }

            private IEnumerable<SimpleQueryClauseBase> ApplyWhereClauses(IEnumerable<SimpleQueryClauseBase> clauses)
            {
                var list = clauses.OfType<WhereClause>().ToList();

                if (list.Count == 1)
                { 
                    Criteria = list[0].Criteria;
                }
                else if (list.Count > 1)
                {
                    var criteria = new SimpleExpression(list[0], list[1], SimpleExpressionType.And);
                    for (int i = 2; i < list.Count; i++)
                        criteria = new SimpleExpression(criteria, list[i], SimpleExpressionType.And);

                    Criteria = criteria;
                }

                return clauses.Where(x => !list.Contains(x));
            }
        }
    }
}