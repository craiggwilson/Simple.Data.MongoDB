using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.MongoDBTest
{
    [TestFixture]
    public class EmptyFindTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Empty();
        }

        [Test]
        public void TestFindById()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(1);
            Assert.IsNull(user);
        }

        [Test]
        public void TestFindAll()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.All();
            Assert.IsEmpty(user);
        }

        [Test]
        public void TestFindAllByName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAllByName("Bob").Cast<User>();
            Assert.AreEqual(0, users.Count());
        }

        [Test]
        public void TestAnd()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age > 32 && db.Users.Name == "Dave").Cast<User>();
            Assert.AreEqual(0, users.Count());
        }
        
        [Test]
        public void TestQuery()
        {
            var db = DatabaseHelper.Open();
            List<dynamic> users = db.Users.Query()
                .Select(db.Users.Name.As("FirstName"), db.Users.Address.City.As("cty"))
                .OrderByDescending(db.Users.Age)
                .ThenBy(db.Users.Name)
                .Skip(1)
                .Take(2)
                .ToList();
            Assert.AreEqual(0, users.Count());
        }

        [Test]
        public void TestRange()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAllByAge(32.to(48)).Cast<User>();
            Assert.AreEqual(0, users.Count());
        }

        [Test]
        public void TestAllCount()
        {
            var db = DatabaseHelper.Open();
            var count = db.Users.All().ToList().Count;
            Assert.AreEqual(0, count);
        }
    }
}