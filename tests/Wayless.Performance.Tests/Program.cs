﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mapster;
using Wayless.Core;

namespace Wayless.Performance.Tests
{
    public sealed class Person
    {
        public bool Index { get { return true; } }
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
                Phone = "8675309 "
            };
        }
    }

    public sealed class PersonDTO
    {
        public int Index { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public DateTime CreateTime { get; set; }
        public string Phone { get; set; }
    }

    public sealed class PersonDTONested
    {
        public int Index { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public DateTime CreateTime { get; set; }
        public string Phone { get; set; }

        public PersonDTO NestedPersonDTO { get; set; }

        public static PersonDTONested Create()
        {
            return new PersonDTONested
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "tests@gitTest.com",
                Address = "Test Street",
                CreateTime = DateTime.Now,
                Nickname = "Jenny",
                Phone = "8675309 "
            };
        }
    }

    public sealed class PersonNested
    {
        public int Index { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public DateTime CreateTime { get; set; }
        public string Phone { get; set; }

        public Person NestedPerson { get; set; }

        public static PersonNested Create()
        {
            return new PersonNested
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "tests@gitTest.com",
                Address = "Test Street",
                CreateTime = DateTime.Now,
                Nickname = "Jenny",
                Phone = "8675309 "
            };
        }
    }

    public class Program
    {
        private static int Iterations = 1000;
        private static readonly WayMore _waymore = new WayMore();


        static void Main(string[] args)
        {
            BenchmarkRunner.Run<LessBenchmarks>();
            
            // TestNewConfiguration();

            // // primer call to cache and compile expressions
            // Console.WriteLine("Basic mapping\r\n");
            // Console.WriteLine("------------------------------------");
            // Console.WriteLine("Primer call\r\n");
            // RunMappers();

            // for (int i = 1; i <= 100; i++)
            // {
            //     //Iterations = Iterations * 10;

            //     Console.WriteLine($"Test iteration: {i}");
            //     Console.WriteLine($"Set size: {Iterations}\r\n");
            //     RunMappers();
            // }

            Console.ReadLine();
        }

        public static void TestNewConfiguration()
        {
            var person = Person.Create();
            _waymore
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldMap(x => x.FirstName, s => s.Nickname, s => s.Phone == "1112223344")
                   .FinalizeRules();
            });

            var personDto0 = _waymore.Map<PersonDTO, Person>(person);


            _waymore
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldMap(x => x.Nickname, s => s.FirstName)
                   .FieldMap(x => x.FirstName, s => s.Nickname)
                   .FinalizeRules();
            });

            var personDto = _waymore.Map<PersonDTO, Person>(person);

            _waymore
            .SetRules<PersonDTO, Person>(cfg =>
            {
                cfg.FieldMap(x => x.FirstName, s => s.Nickname)
                   .FieldMap(x => x.Nickname, s => s.FirstName)
                   .FinalizeRules();
            });

            var personDto2 = _waymore.Map<PersonDTO, Person>(person);
        }

        private static void RunMappers()
        {
            MeasureManualMap();
            MeasureAutoMapper();
            MesaureMapster();
            MeasureWaylessInstance();
            MeasureCachedWaylessInstanceNestedMap();

            Console.WriteLine("\r\n-------------ENUMERABLE MAP-----------------------\r\n");
            MeasureWaylessEnumerableMap();
            MeasureMapsterEnumerableMap();


            var person = Person.Create();
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("\r\n------------------SINGLE OBJECT------------------\r\n");

            stopwatch.Start();
            person.Adapt<PersonDTO>();
            stopwatch.Stop();
            Console.WriteLine("Mapster: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Reset();


            stopwatch.Start();
            _waymore.Map<PersonDTO, Person>(person);
            stopwatch.Stop();
            Console.WriteLine("_waymore: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Reset();


            stopwatch.Start();
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDTO>());
            var mapper = config.CreateMapper();
            var personDto = mapper.Map<Person, PersonDTO>(person);
            stopwatch.Stop();
            Console.WriteLine("Automapper: {0}ms", stopwatch.Elapsed.TotalMilliseconds);

            Console.WriteLine("\r\n------------------------------------\r\n");
        }

        public static void MeasureManualMap()
        {
            Person person = Person.Create();

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                PersonDTO personDto = ManualMap(person);
            }
            stopwatch.Stop();
            Console.WriteLine("Manual: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        public static void MeasureAutoMapper()
        {
            Person person = Person.Create();
            Stopwatch stopwatch = Stopwatch.StartNew();
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDTO>());
            var mapper = config.CreateMapper();

            stopwatch.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map<Person, PersonDTO>(person);
            }
            stopwatch.Stop();
            Console.WriteLine("AutoMapper: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        public static void MeasureWaylessInstance()
        {
            Person person = Person.Create();

            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = _waymore.Get<PersonDTO, Person>();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless : {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureWaylessEnumerableMap()
        {
            var persons = new List<Person>();
            for (int i = 0; i < Iterations; i++)
            {
                persons.Add(Person.Create());
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            var personDtos = _waymore.Map<PersonDTO, Person>(persons);

            stopwatch.Stop();
            Console.WriteLine("Wayless Set Map: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureMapsterEnumerableMap()
        {
            var persons = new List<Person>();
            for (int i = 0; i < Iterations; i++)
            {
                persons.Add(Person.Create());
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            var personDtos = persons.Adapt<IEnumerable<PersonDTO>>().ToList();

            stopwatch.Stop();
            Console.WriteLine("Mapster Set Map: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureCachedWaylessInstanceNestedMap()
        {
            PersonNested person = PersonNested.Create();
            person.NestedPerson = Person.Create();

            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = _waymore
                                .SetRules<PersonDTONested, PersonNested>(rules =>
                                {
                                    var nm = _waymore.Get<PersonDTO, Person>();
                                    rules.FieldMap(x => x.NestedPersonDTO, x => nm.Map(x.NestedPerson));
                                })
                                .Get<PersonDTONested, PersonNested>();

            for (int i = 0; i < Iterations; i++)
            {
                var personDtoNested = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless (Nested): {0}ms", stopwatch.Elapsed.TotalMilliseconds);

            Stopwatch stopwatch2 = Stopwatch.StartNew();

            var nestedMapper = _waymore.Get<PersonDTO, Person>();
            var mapper2 = _waymore
                                 .SetRules<PersonDTONested, PersonNested>(rules =>
                                 {
                                     rules.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson))
                                          .FinalizeRules();
                                 })
                                 .Get<PersonDTONested, PersonNested>();


            for (int i = 0; i < Iterations; i++)
            {
                var personDtoNested = mapper2.Map(person);
            }

            stopwatch2.Stop();
            Console.WriteLine("Wayless (Nested2): {0}ms", stopwatch2.Elapsed.TotalMilliseconds);
        }

        public static void MesaureMapster()
        {
            Person person = Person.Create();
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                var personDto = person.Adapt<PersonDTO>();
            }

            stopwatch.Start();
            Console.WriteLine("Mapster: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
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
