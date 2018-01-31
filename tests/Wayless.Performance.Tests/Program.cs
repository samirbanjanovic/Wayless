﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Mapster;

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

    class Program
    {
        private static int Iterations = 1000;

        static void Main(string[] args)
        {
            // primer call to cache and compile expressions
            Console.WriteLine("Basic mapping\r\n");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Primer call\r\n");
            RunMappers();

            for (int i = 1; i <= 5; i++)
            {
                //Iterations = Iterations * 10;

                Console.WriteLine($"Test iteration: {i}");
                Console.WriteLine($"Set size: {Iterations}\r\n");
                RunMappers();
            }

            Console.ReadLine();
        }

        private static void RunMappers()
        {
            MeasureManualMap();
            MeasureAutoMapper();            
            MesaureMapster();
            MeasureNewWaylessInstance();
            MeasureCachedWaylessInstance();

            Console.WriteLine("\r\n------------------------------------\r\n");
        }

        private static void MeasureManualMap()
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

        private static void MeasureAutoMapper()
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

        private static void MeasureNewWaylessInstance()
        {
            Person person = Person.Create();

            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = WayMore.Mappers.GetNew<PersonDTO, Person>();
            
            stopwatch.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless (new instance): {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureCachedWaylessInstance()
        {
            Person person = Person.Create();            
            Stopwatch stopwatch = Stopwatch.StartNew();

            var config = WaylessConfigurationBuilder.GetDefaultConfiguration<PersonDTO, Person>();
            var mapper = WayMore.Mappers.Get<PersonDTO, Person>(config);

            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless (cached instance): {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MesaureMapster()
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
        {
            var result = new PersonDTO
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
            return result;
        }

    }
}
