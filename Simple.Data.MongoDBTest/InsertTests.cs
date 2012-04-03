using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.MongoDBTest
{
    [TestFixture]
    public class InsertTests
    {
        [SetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestInsertWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            dynamic address = new ExpandoObject();
            address.Line = "456 Way";
            db.Users.Insert(Id: 4, Name: "Ford", Password: "hoopy", Age: 29, Address: address, EmailAddresses: new List<string> { "jack@mcjack.com", "bob@mcbob.com" });

            var user = db.Users.FindById(4);

            Assert.IsNotNull(user);
            Assert.AreEqual("Ford", user.Name);
            Assert.AreEqual("hoopy", user.Password);
            Assert.AreEqual(29, user.Age);
            Assert.IsNotNull(user.EmailAddresses);
            Assert.AreEqual("bob@mcbob.com", user.EmailAddresses[1]);
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User { Id = 4, Name = "Zaphod", Password = "zarquon", Age = 42, Address = new Address { State = "TX" }, EmailAddresses = new List<string> { "jack@mcjack.com", "bob@mcbob.com" } };

            db.Users.Insert(user);

            User actual = db.Users.FindById(4);

            Assert.IsNotNull(user);
            Assert.AreEqual("Zaphod", actual.Name);
            Assert.AreEqual("zarquon", actual.Password);
            Assert.AreEqual(42, actual.Age);
            Assert.IsNotNull(user.EmailAddresses);
            Assert.AreEqual("bob@mcbob.com", user.EmailAddresses[1]);
        }

        [Test]
        public void TestInsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Id = 4;
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;
            user.Address = new ExpandoObject();
            user.Address.City = "Los Angeles";
            user.EmailAddresses = new List<string> { "jack@mcjack.com", "bob@mcbob.com" };

            db.Users.Insert(user);

            var actual = db.Users.FindById(4);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }
    }
}
