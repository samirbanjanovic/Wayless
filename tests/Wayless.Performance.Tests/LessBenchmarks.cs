using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Mapster;

namespace Wayless.Performance.Tests
{
    [CoreJob]
    [MinColumn, MaxColumn]
    [MarkdownExporter, HtmlExporter]
    public class LessBenchmarks
    {
        private static int Iterations = 100;
                
        private static readonly WayMore _waymore = new WayMore();

        private static readonly Person person = Person.Create();

        [Benchmark(Baseline = true)]
        public void MeasureManualMap()
        {
            //Person person = Person.Create();

            for (int i = 0; i < Iterations; i++)
            {
                PersonDTO personDto = ManualMap(person);
            }
        }

        //[Benchmark]
        //public void MeasureAutoMapper()
        //{
        //    //Person person = Person.Create();
        //    var config = new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDTO>());
        //    var mapper = config.CreateMapper();

        //    for (int i = 0; i < Iterations; i++)
        //    {
        //        var personDto = mapper.Map<Person, PersonDTO>(person);
        //    }
        //}

        [Benchmark]
        public void MeasureWaylessInstance()
        {
            //Person person = Person.Create();

            var mapper = _waymore.Get<PersonDTO, Person>();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map(person);
            }
        }

        [Benchmark]
        public void MesaureMapster()
        {
            //Person person = Person.Create();

            for (int i = 0; i < Iterations; i++)
            {
                var personDto = person.Adapt<PersonDTO>();
            }
        }

        private static PersonDTO ManualMap(Person person)
            => new PersonDTO
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                Address = person.Address,
                CreateTime = person.CreateTime,
                Nickname = person.Nickname,
                Phone = person.Phone

            };
    }
}
