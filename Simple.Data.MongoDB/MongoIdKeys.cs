using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Simple.Data.Extensions;

namespace Simple.Data.MongoDB
{
    public static class MongoIdKeys
    {
        private static readonly string[] _idCandidates = new[] { "_id", "Id", "id", "ID" };

        public static readonly IEqualityComparer<string> Comparer = new MongoIdKeyComparer();

        public static KeyValuePair<string, object>? FindId(IDictionary<string, object> data)
        {
            for (int i = 0; i < _idCandidates.Length; i++)
            {
                var currentCandidate = _idCandidates[i];
                if (data.ContainsKey(currentCandidate))
                {
                    return new KeyValuePair<string, object>(currentCandidate, data[currentCandidate]);
                }
            }

            return null;
        }

        public static void ReplaceId(IDictionary<string, object> data)
        {
            var idPair = FindId(data);
            if(idPair.HasValue)
            {
                data.Remove(idPair.Value.Key);
                data.Add("_id", idPair.Value.Value);
            }
        }

        private class MongoIdKeyComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                if (_idCandidates.Contains(x))
                    return x.Homogenize() == y.Homogenize();

                return x == y;
            }

            public int GetHashCode(string obj)
            {
                if (_idCandidates.Contains(obj))
                    return obj.Homogenize().GetHashCode();

                return obj.GetHashCode();
            }
        }
    }
}