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
    [MarkdownExporter]
    [MemoryDiagnoser]
    public class LessBenchmarks
    {
        private static int Iterations = 100;
                
        private static readonly WayMore _waymore = new WayMore();

        private readonly Person person = Person.Create();
        private readonly PersonNested personNested = GetPersonNested();

        private readonly Core.IWayless<PersonDTONested, PersonNested> inFuncMapper
            = _waymore.SetRules<PersonDTONested, PersonNested>(rules =>
            {
                var nm = _waymore.Get<PersonDTO, Person>();
                rules.FieldMap(x => x.NestedPersonDTO, x => nm.Map(x.NestedPerson));
            })
                        .Get<PersonDTONested, PersonNested>();

        private readonly Core.IWayless<PersonDTONested, PersonNested> outOfFuncMapper = GetOutOfFuncMapper();

        [Benchmark(Baseline = true)]
        public void Manual()
        {
            for (int i = 0; i < Iterations; i++)
            {
                ManualMap(person);
            }
        }

        [Benchmark]
        public void AutoMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDTO>());
            var mapper = config.CreateMapper();

            for (int i = 0; i < Iterations; i++)
            {
                mapper.Map<Person, PersonDTO>(person);
            }
        }

        [Benchmark]
        public void Wayless()
        {
            var mapper = _waymore.Get<PersonDTO, Person>();

            for (int i = 0; i < Iterations; i++)
            {
                mapper.Map(person);
            }
        }
        
        [Benchmark]
        public void WaylessNested_InFuncMapper()
        {
            for (int i = 0; i < Iterations; i++)
            {
                inFuncMapper.Map(personNested);
            }
        }
        
        [Benchmark]
        public void WaylessNested_OutOfFuncMapper()
        {           
            for (int i = 0; i < Iterations; i++)
            {
                outOfFuncMapper.Map(personNested);
            }
        }

        private static Core.IWayless<PersonDTONested, PersonNested> GetOutOfFuncMapper()
        {
            var nestedMapper = _waymore.Get<PersonDTO, Person>();
            var mapper2 = _waymore.SetRules<PersonDTONested, PersonNested>(rules =>
                                     {
                                         rules.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson))
                                              .FinalizeRules();
                                     })
                                     .Get<PersonDTONested, PersonNested>();

            return mapper2;
        }


        [Benchmark]
        public void MapsterNested()
        {
            for (int i = 0; i < Iterations; i++)
            {
                person.Adapt<PersonDTO>();
            }
        }

        [Benchmark]
        public void Mapster()
        {
            for (int i = 0; i < Iterations; i++)
            {
                personNested.Adapt<PersonDTONested>();
            }
        }

        private static PersonNested GetPersonNested()
        {
            PersonNested personNested = PersonNested.Create();
            personNested.NestedPerson = Person.Create();

            return personNested;
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
