using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wayless.Tests
{
    [TestClass]
    public class WaylessMapTests
    {
        public class Source
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public DateTime TimeStamp { get; set; }
        }

        public class Destination
        {
            public string Name { get; set; }

            public DateTime DateTime { get; set; }
        }

        public Source SourceObjcet = new Source()
        {
            Id = 1,
            Name = "Source",
            TimeStamp = DateTime.Now.AddDays(-1)
        };

        [TestMethod]
        public void TestDefaultMap()
        {
            var mapper = new WaylessMap<Source, Destination>();
            var destination = mapper.Map(SourceObjcet);

            Assert.AreEqual(SourceObjcet.Name, destination.Name);
            Assert.AreNotEqual(SourceObjcet.TimeStamp, destination.DateTime);
        }

        [TestMethod]
        public void TestExplicitMap()
        {
            var mapper = new WaylessMap<Source, Destination>()
                                .Explicit(s => s.TimeStamp, d => d.DateTime);

            var destination = mapper.Map(SourceObjcet);

            Assert.AreEqual(SourceObjcet.Name, destination.Name);
            Assert.AreEqual(SourceObjcet.TimeStamp, destination.DateTime);
        }

        [TestMethod]
        public void TestIgnoreMap()
        {
            var mapper = new WaylessMap<Source, Destination>()
                                .Ignore(x => x.Name);

            var destination = mapper.Map(SourceObjcet);

            Assert.AreNotEqual(SourceObjcet.Name, destination.Name);
        }

        [TestMethod]
        public void TestExplicitWithIgnore()
        {
            var mapper = new WaylessMap<Source, Destination>()
                                .Explicit(s => s.TimeStamp, d => d.DateTime)
                                .Ignore(x => x.Name);

            var destination = mapper.Map(SourceObjcet);

            Assert.AreEqual(SourceObjcet.TimeStamp, destination.DateTime);
            Assert.AreNotEqual(SourceObjcet.Name, destination.Name);
        }

        [TestMethod]
        public void TestShowMappingDictionary()
        {
            var mapper = new WaylessMap<Source, Destination>();

            var mapping = mapper.ShowMapping();
        }

        [TestMethod]
        public void TestExplicitAssignments()
        {
            var destination = new WaylessMap<Source, Destination>(SourceObjcet)
                                        .Explicit((s,d) =>
                                        {
                                            d.Name = s.Name;
                                            d.DateTime = s.TimeStamp;
                                        })
                                        .Map();

            Assert.AreEqual(SourceObjcet.TimeStamp, destination.DateTime);

            destination = new WaylessMap<Source, Destination>(SourceObjcet)
                                        .Explicit(d =>
                                        {
                                            d.Name = "Explicit2";
                                        })
                                        .Explicit(s => s.TimeStamp, d => d.DateTime)
                                        .Map();
        }
    }
}
