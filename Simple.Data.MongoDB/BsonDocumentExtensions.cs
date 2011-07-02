using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;

namespace Simple.Data.MongoDB
{
    public static class BsonDocumentExtensions
    {
        public static IDictionary<string, object> ToDictionary(this BsonDocument document)
        {
            return ToDictionary(document, new Dictionary<string, string>());
        }

        public static IDictionary<string, object> ToDictionary(this BsonDocument document, IDictionary<string, string> aliases)
        {
            return document.Elements.ToDictionary(x => aliases.ContainsKey(x.Name) ? aliases[x.Name] : x.Name, x => ConvertValue(x.Value), MongoIdKeyComparer.DefaultInstance);
        }

        private static object ConvertValue(BsonValue value)
        {
            if (value.IsBsonDocument)
                return value.AsBsonDocument.ToDictionary();
            if (value.IsBsonArray)
                return value.AsBsonArray.Select(v => ConvertValue(v)).ToList();

            return value.RawValue;
        }
    }
}