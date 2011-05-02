using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

using MongoDB.Bson.Serialization;

namespace Simple.Data.MongoDB
{
    public class DynamicSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
                return new DynamicBsonSerializer();

            return null;
        }
    }
}