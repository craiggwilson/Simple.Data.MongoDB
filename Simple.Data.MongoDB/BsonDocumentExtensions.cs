using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace Simple.Data.MongoDB
{
    public static class BsonDocumentExtensions
    {
        public static IDictionary<string, object> ToSimpleDictionary(this BsonDocument document)
        {
            return ToSimpleDictionary(document, new Dictionary<string, string>());
        }

        public static IDictionary<string, object> ToSimpleDictionary(this BsonDocument document, IDictionary<string, string> aliases)
        {
            if (document == null) return null;

            return document.Elements.ToDictionary(x => aliases.ContainsKey(x.Name) ? aliases[x.Name] : x.Name, x => ConvertValue(x.Name, x.Value, aliases), MongoIdKeyComparer.DefaultInstance);
        }

        private static object ConvertValue(string elementName, BsonValue value, IDictionary<string, string> aliases)
        {
            if (value.IsBsonDocument)
            {
                aliases = aliases.Where(x => x.Key.StartsWith(elementName + ".")).ToDictionary(x => x.Key.Remove(0, elementName.Length + 1), x => x.Value);
                return value.AsBsonDocument.ToSimpleDictionary(aliases);
            }
            else if (value.IsBsonArray)
                return value.AsBsonArray.Select(v => ConvertValue(elementName, v, aliases)).ToList();
            else if (value.IsBoolean)
                return value.AsBoolean;
            else if (value.IsDateTime)
                return value.AsDateTime;
            else if (value.IsDouble)
                return value.AsDouble;
            else if (value.IsGuid)
                return value.AsGuid;
            else if (value.IsInt32)
                return value.AsInt32;
            else if (value.IsInt64)
                return value.AsInt64;
            else if (value.IsObjectId)
                return value.AsObjectId;
            else if (value.IsString)
                return value.AsString;

            return value.RawValue;
        }
    }
}