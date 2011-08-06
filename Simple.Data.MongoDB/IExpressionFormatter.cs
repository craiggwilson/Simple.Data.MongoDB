using System.Collections.Generic;
using MongoDB.Driver.Builders;

namespace Simple.Data.MongoDB
{
    public interface IExpressionFormatter
    {
        QueryComplete Format(SimpleExpression expressions);
    }
}