using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wayless;

namespace Wayless.Tests
{
    [TestClass]
    public class WaylessMapTests
    {
        public class Person
        {
            public string Address { get; set; }
            public DateTime CreateTime { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public Guid Id { get; set; }
            public string LastName { get; set; }
            public string Nickname { get; set; }
            public string Phone { get; set; }
            public static Person Create()
            {
                return new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "tests@gitTest.com",
                    Address = "Test Street",
                    CreateTime = DateTime.Now,
                    Nickname = "Jenny",
                    Phone = "1112223344 "
                };
            }
        }

        public class PersonDTO
        {
            public string Address { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public Guid Id { get; set; }
            public string LastName { get; set; }
            public string Nickname { get; set; }
            public DateTime CreateTime { get; set; }

            public string Phone;
        }

        #region FieldMap Tests
        [TestMethod]
        public void TestDefaultInitializeAndMap()
        {
            var person = Person.Create();
            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual(person.Address, personDto.Address);
            Assert.AreEqual(person.Email, personDto.Email);
            Assert.AreEqual(person.FirstName, personDto.FirstName);
            Assert.AreEqual(person.Id, personDto.Id);
            Assert.AreEqual(person.LastName, personDto.LastName);
            Assert.AreEqual(person.Nickname, personDto.Nickname);
            Assert.AreEqual(person.CreateTime, personDto.CreateTime);
            Assert.AreEqual(person.Phone, personDto.Phone);
        }

        [TestMethod]
        public void TestFieldMap()
        {
            var person = Person.Create();

            WayMore.Wayless
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldMap(x => x.Nickname, s => s.FirstName)
                   .FieldMap(x => x.FirstName, s => s.Nickname);
            });

            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual(person.Nickname, personDto.FirstName);
            Assert.AreEqual(person.FirstName, personDto.Nickname);
        }

        [TestMethod]
        public void TestFieldMapWithCondition()
        {
            var person = Person.Create();
            WayMore.Wayless
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldMap(x => x.FirstName, s => s.Nickname, s => s.Phone == "1112223344");
            });

            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual(person.Nickname, personDto.Nickname);
        }

        [TestMethod]
        public void TestFieldMapWithoutAutoMatch()
        {
            var person = Person.Create();
            var mapper = WayMore.Wayless
                                .Get<PersonDTO, Person>();
            WayMore.Wayless
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldMap(x => x.Nickname, s => s.FirstName)
                    .FieldMap(x => x.FirstName, s => s.Nickname)
                    .FieldMap(x => x.Phone, s => s.Phone, s => s.Nickname == "Jenny")
                    .FieldSet(x => x.Address, "1234 ABC")
                    .FieldSet(x => x.Id, Guid.NewGuid())
                    .FieldMap(x => x.LastName, s => s.FirstName)
                    .FieldSet(x => x.CreateTime, DateTime.Now)
                    .FieldSkip(x => x.Email);
            });

            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual(person.FirstName, personDto.Nickname);
            Assert.AreEqual(person.Nickname, personDto.FirstName);
            Assert.AreEqual(person.Phone, personDto.Phone);

            Assert.AreNotEqual(person.Address, personDto.Address);
            Assert.AreNotEqual(person.Email, personDto.Email);
            Assert.AreNotEqual(person.Id, personDto.Id);
            Assert.AreNotEqual(person.LastName, personDto.LastName);
            Assert.AreNotEqual(person.CreateTime, personDto.CreateTime);
        }
        #endregion FieldMap Tests

        #region FieldSet Tests

        [TestMethod]
        public void TestFieldSet()
        {
            var person = Person.Create();
            WayMore.Wayless
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldSet(x => x.Nickname, "Jacqueline");
            });

            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual("Jacqueline", personDto.Nickname);
        }

        [TestMethod]
        public void TestFieldSetWithCondition()
        {
            var person = Person.Create();
            WayMore
                .Wayless
                .SetRules<PersonDTO, Person>(cfg => cfg.FieldSet(x => x.Phone, "8675309", x => x.Nickname == "Jenny"));


            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual("8675309", personDto.Phone);
        }

        [TestMethod]
        public void TestFieldSetWithoutAutoMatch()
        {
            var person = Person.Create();
            WayMore.Wayless
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldSet(x => x.Phone, "8675309", x => x.Nickname == "Jenny");
            });

            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreEqual("8675309", personDto.Phone);
        }

        #endregion FieldSet Tests

        #region FieldSkip Tests

        [TestMethod]
        public void TestFieldSkip()
        {
            var person = Person.Create();
            WayMore.Wayless
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldSkip(d => d.Nickname);
            });

            var personDto = WayMore.Wayless.Map<PersonDTO, Person>(person);

            Assert.AreNotEqual(person.Nickname, personDto.Nickname);
        }

        #endregion FieldSkip Tests
    }
}

