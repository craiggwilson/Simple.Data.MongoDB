using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Simple.Data.Extensions;

namespace Simple.Data.MongoDB
{
    public class MongoIdKeyComparer : IEqualityComparer<string>
    {
        private static readonly string[] _idCandidates = new[] { "_id", "Id", "id", "ID" };

        public static readonly MongoIdKeyComparer DefaultInstance = new MongoIdKeyComparer();

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