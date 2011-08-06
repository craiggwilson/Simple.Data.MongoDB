using System.Collections.Generic;
using System.Linq;
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
            if (document == null) return null;

            return document.Elements.ToDictionary(x => aliases.ContainsKey(x.Name) ? aliases[x.Name] : x.Name, x => ConvertValue(x.Name, x.Value, aliases), MongoIdKeyComparer.DefaultInstance);
        }

        private static object ConvertValue(string elementName, BsonValue value, IDictionary<string, string> aliases)
        {
            if (value.IsBsonDocument)
            {
                aliases = aliases.Where(x => x.Key.StartsWith(elementName + ".")).ToDictionary(x => x.Key.Remove(0, elementName.Length + 1), x => x.Value);
                return value.AsBsonDocument.ToDictionary(aliases);
            }
            if (value.IsBsonArray)
                return value.AsBsonArray.Select(v => ConvertValue(elementName, v, aliases)).ToList();

            return value.RawValue;
        }
    }
}