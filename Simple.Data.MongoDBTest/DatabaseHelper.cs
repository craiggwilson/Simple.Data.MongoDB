using System.Collections.Generic;
using MongoDB.Driver;
using Simple.Data.MongoDB;
using System;

namespace Simple.Data.MongoDBTest
{
    internal static class DatabaseHelper
    {
        public static dynamic Open()
        {
            return Database.Opener.OpenMongo("mongodb://localhost/simpleDataTests?safe=true");
        }

        public static void Reset()
        {
            var server = MongoServer.Create("mongodb://localhost/?safe=true");
            server.Connect();
            server.DropDatabase("simpleDataTests");
            InsertData(server.GetDatabase("simpleDataTests"));
        }

        public static void Empty()
        {
            var server = MongoServer.Create("mongodb://localhost/?safe=true");
            server.Connect();
            server.DropDatabase("simpleDataTests");
        }

        private static void InsertData(MongoDatabase db)
        {
            var users = new[] 
            {
                new User { Id = 1, Name = "Bob", Password = "Bob", Age = 32, LastLoginDate = new DateTime(2009,01,01), Address = new Address { Line = "123 Way", City = "Dallas", State = "TX" }, EmailAddresses = new List<string> { "bob@bob.com", "b@b.com" }, Dependents = new List<Dependent>{ new Dependent { Name = "Jane", Age = 12 }, new Dependent { Name = "Jimmy", Age = 11 } } },
                new User { Id = 2, Name = "Charlie", Password = "Charlie", Age = 49, LastLoginDate = new DateTime(2010,01,01), Address = new Address { Line = "234 Way", City = "San Francisco", State = "CA" }, EmailAddresses = new List<string> { "charlie@charlier.com" }, Dependents = new List<Dependent>{ new Dependent { Name = "Joanne", Age = 12 } }   },
                new User { Id = 3, Name = "Dave", Password = "Dave", Age = 49, LastLoginDate = new DateTime(2011,01,01), Address = new Address { Line = "345 Way", City = "Austin", State = "TX" } }
            };

            db.GetCollection("Users").InsertBatch(users);
        }
    }
}