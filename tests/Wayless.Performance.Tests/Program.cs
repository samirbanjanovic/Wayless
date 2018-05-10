using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
            MeasureCachedWaylessInstanceNestedMap();

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
            var mapper = WayMore.Wayless
                                .GetNew<PersonDTO, Person>();

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
            var config = WaylessConfigurationBuilder.GetEmptyConfiguration()
                                                    .UseDefaultExpressionBuilder(typeof(PersonDTO), typeof(Person))
                                                    .UseDefaultMatchMaker();

            var mapper = WayMore.Wayless.Get<PersonDTO, Person>(config);

            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless (cached instance): {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureCachedWaylessInstanceNestedMap()
        {
            PersonNested person = PersonNested.Create();
            person.NestedPerson = Person.Create();

            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = WayMore.Wayless
                                .Get<PersonDTONested, PersonNested>();

            mapper.FieldMap(x => x.NestedPersonDTO, x => WayMore.Wayless.Get<PersonDTO, Person>().Map(x.NestedPerson));

            for (int i = 0; i < Iterations; i++)
            {
                var personDtoNested = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless (Nested): {0}ms", stopwatch.Elapsed.TotalMilliseconds);

            Stopwatch stopwatch2 = Stopwatch.StartNew();
            var nestedMapper = WayMore.Wayless.Get<PersonDTO, Person>();
            var mapper2 = WayMore.Wayless
                                .Get<PersonDTONested, PersonNested>();
            mapper.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson));

            for (int i = 0; i < Iterations; i++)
            {
                var personDtoNested = mapper2.Map(person);
            }

            stopwatch2.Stop();
            Console.WriteLine("Wayless (Nested2): {0}ms", stopwatch2.Elapsed.TotalMilliseconds);
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
