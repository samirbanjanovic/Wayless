using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;


namespace Wayless.Performance.Tests
{
    public sealed class Person
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
                FirstName = "John",
                LastName = "Doe",
                Email = "support@tinymapper.net",
                Address = "Wall Street",
                CreateTime = DateTime.Now,
                Nickname = "Object Mapper",
                Phone = "Call Me Maybe "
            };
        }
    }

    public sealed class PersonDTO
    {
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
        private const int Iterations = 100000;

        static void Main(string[] args)
        {
            while(true)
            {                
                MeasureManualMap();
                MeasureAutoMapper();
                MeasureWaylessMap();
                
                Console.ReadLine();
            }            
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
            Console.WriteLine("Handwritten: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeasureAutoMapper()
        {
            Person person = Person.Create();            
            Stopwatch stopwatch = Stopwatch.StartNew();
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Person, PersonDTO>());
            var mapper = config.CreateMapper();
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper.Map<Person, PersonDTO>(person);
            }
            stopwatch.Stop();
            Console.WriteLine("AutoMapper: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }


        private static void MeasureWaylessMap()
        {
            Person person = Person.Create();
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            var mapper = new WaylessMap<PersonDTO, Person>();
                        
                            
            for (int i = 0; i < Iterations; i++)
            {
                var personDto = mapper                                
                                .Map(person);
                                       
            }

            stopwatch.Stop();
            Console.WriteLine("Wayless: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
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
