using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wayless.Tests
{
    [TestClass]
    public class WaylessMapTests
    {
        public class SourceObject
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public DateTime TimeStamp { get; set; }

            public Guid CorrelationId { get; set; }
        }

        public class DestinationObject
        {
            public string Name { get; set; }

            public DateTime AssignmentDate { get; set; }

            public string CorrelationId { get; set; }
        }

        public SourceObject TestSource = new SourceObject()
        {
            Id = 1,
            Name = "Source",
            TimeStamp = DateTime.Now.AddDays(-1)
        };

        [TestMethod]
        public void TestDefaultMap()
        {
            var mapper = new WaylessMap<SourceObject, DestinationObject>();


            var destination = mapper.Map(TestSource);

            Assert.AreEqual(TestSource.Name, destination.Name);
            Assert.AreEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        [TestMethod]
        public void TestFieldMap()
        {
            var mapper = new WaylessMap<SourceObject, DestinationObject>()
                                .FieldMap(d => d.AssignmentDate, s => s.TimeStamp);

            var destination = mapper.Map(TestSource);

            Assert.AreEqual(TestSource.Name, destination.Name);
            Assert.AreEqual(TestSource.TimeStamp, destination.AssignmentDate);
            Assert.AreEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        [TestMethod]
        public void TestFieldSet()
        {
            var mapper = new WaylessMap<SourceObject, DestinationObject>()
                                .FieldSet(d => d.CorrelationId, Guid.NewGuid());

            var destination = mapper.Map(TestSource);

            Assert.AreNotEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        [TestMethod]
        public void TestSkipField()
        {
            var mapper = new WaylessMap<SourceObject, DestinationObject>()
                                .FieldSkip(d => d.CorrelationId);

            var destination = mapper.Map(TestSource);

            Assert.AreNotEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }
    }
}

