using System.Collections.Generic;
using MongoDB.Driver;

namespace Simple.Data.MongoDB
{
    public interface IExpressionFormatter
    {
        IMongoQuery Format(SimpleExpression expressions);
    }
}