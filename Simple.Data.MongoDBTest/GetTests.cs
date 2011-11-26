using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Simple.Data.MongoDB;

namespace Simple.Data.MongoDBTest
{
    /// <summary>
    /// Summary description for GetTests
    /// </summary>
    [TestFixture]
    public class GetTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestGet()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Get(1);
            Assert.AreEqual(1, user.Id);
        }
    }
}