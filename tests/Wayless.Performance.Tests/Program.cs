using System;
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
        {            return new Person
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
        public string Phone;
    }

    class Program
    {
        private const int Iterations = 1000;

        static void Main(string[] args)
        {
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"Test iteration: {i}");
                Console.WriteLine($"Set size: {Iterations}");
                MeasureManualMap();
                MeasureAutoMapper();
                MeasureWayless();
                MeasureWaylessWithWayMore();
                MesaureMapster();

                Console.WriteLine("------------------------------------");
            }

            Console.ReadLine();
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
            stopwatch.Stop();
            Console.WriteLine("AutoMapper Init: {0}ms", stopwatch.Elapsed.TotalMilliseconds);

            stopwatch.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map<Person, PersonDTO>(person);
            }
            stopwatch.Stop();
            Console.WriteLine("AutoMapper: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureWayless()
        {
            Person person = Person.Create();
            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = WayMore.Mappers.GetNew<PersonDTO, Person>(); //new WaylessMap<PersonDTO, Person>();

            stopwatch.Stop();
            Console.WriteLine("Wayless Init: {0}ms", stopwatch.Elapsed.TotalMilliseconds);

            stopwatch.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        //private static readonly IWaylessMap<PersonDTO, Person> _staticWaylessMapper = new WaylessMap<PersonDTO, Person>();
        private static void MeasureWaylessWithWayMore()
        {
            Person person = Person.Create();
            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = WayMore.Mappers.Get<PersonDTO, Person>();
            mapper.FieldMap(x => x.CreateTime, s => s.CreateTime);
            
            //mapper.FieldMap(x => x.Nickname, s => $"{s.LastName}, {s.FirstName}, {s.Id}", z => z.Index);

            for (int i = 0; i < Iterations; i++)
            {

                var personDto = mapper.Map(person);
            }

            stopwatch.Stop();
            Console.WriteLine("Static Wayless: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MesaureMapster()
        {
            Person person = Person.Create();
            Stopwatch stopwatch = Stopwatch.StartNew();
            TypeAdapterConfig<Person, PersonDTO>
                    .NewConfig()
                    .Map(x => x.Nickname
                       , s => $"{s.LastName}, {s.FirstName}, {s.Id}"
                       , s => s.Index)
                    .Compile();

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
