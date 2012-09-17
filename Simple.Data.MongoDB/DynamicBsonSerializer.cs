using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.CSharp.RuntimeBinder;

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Simple.Data.MongoDB
{
    public class DynamicBsonSerializer : BsonBaseSerializer
    {
        private static DynamicBsonSerializer singleton = new DynamicBsonSerializer();

        public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            // this is unnecessary in the context of Simple.Data.
            throw new NotImplementedException();
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            if (value == null)
            {
                bsonWriter.WriteNull();
                return;
            }
            var metaObject = ((IDynamicMetaObjectProvider)value).GetMetaObject(Expression.Constant(value));
            var memberNames = metaObject.GetDynamicMemberNames().ToList();
            if (memberNames.Count == 0)
            {
                bsonWriter.WriteNull();
                return;
            }

            bsonWriter.WriteStartDocument();
            foreach (var memberName in memberNames)
            {
                bsonWriter.WriteName(memberName);
                var memberValue = BinderHelper.GetMemberValue(value, memberName);
                if (memberValue == null)
                    bsonWriter.WriteNull();
                else
                {
                    var memberType = memberValue.GetType();
                    var serializer = BsonSerializer.LookupSerializer(memberType);
                    serializer.Serialize(bsonWriter, memberType, memberValue, options);
                }
            }
            bsonWriter.WriteEndDocument();
        }

        private static class BinderHelper
        {
            private static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>> _cache = new ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>>();

            public static object GetMemberValue(object owner, string memberName)
            {
                var getSite = _cache.GetOrAdd(
                    memberName,
                    key => CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, key, typeof(BinderHelper), new [] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
                
                return getSite.Target(getSite, owner);
            }

        }
    }
}