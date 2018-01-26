using System;
using System.Diagnostics;
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

            public Guid CorrelationId { get; set; }
        }

        public SourceObject TestSource = new SourceObject()
        {
            Id = 1,
            Name = "Source",
            CorrelationId = Guid.NewGuid(),
            TimeStamp = DateTime.Now.AddDays(-1)
        };

        [TestMethod]
        public void TestDefaultMap()
        {
            var mapper = new Wayless<DestinationObject, SourceObject>();
            var destination = mapper.Map(TestSource);

            Assert.AreEqual(TestSource.Name, destination.Name);
            //Assert.AreEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        [TestMethod]
        public void TestFieldMap()
        {
            var mapper = new Wayless<DestinationObject, SourceObject>()
                                .FieldMap(d => d.AssignmentDate, s => s.TimeStamp);

            var destination = mapper.Map(TestSource);

            Assert.AreEqual(TestSource.Name, destination.Name);
            Assert.AreEqual(TestSource.TimeStamp, destination.AssignmentDate);
            //Assert.AreEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        [TestMethod]
        public void TestFieldSet()
        {
            var mapper = new Wayless<DestinationObject, SourceObject>()
                                .FieldSet(d => d.CorrelationId, Guid.NewGuid());

            var destination = mapper.Map(TestSource);

            Assert.AreNotEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        [TestMethod]
        public void TestSkipField()
        {
            var mapper = new Wayless<DestinationObject, SourceObject>()
                                .FieldSkip(d => d.CorrelationId);

            var destination = mapper.Map(TestSource);

            Assert.AreNotEqual(TestSource.CorrelationId.ToString(), destination.CorrelationId);
        }

        //[TestMethod]
        //public void TestShowMapping()
        //{
        //    var mapper = new WaylessMap<DestinationObject, SourceObject>()
        //                        .FieldMap(d => d.AssignmentDate, s => s.TimeStamp); 
                                
        //    var mappingRules = mapper.ShowMapping();

        //    Debug.Write(string.Join(Environment.NewLine, mappingRules));
        //}
    }
}

