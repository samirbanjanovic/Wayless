Wayless is a basic, lightweight object-to-object mapper.

There are multiple ways to get an instance of a Wayless mapper. The most basic 
way is to use the `WayMore` singleton.  It keeps track of all requested mappers
based on destinatoin type, source type, and expression builder.  

`Wayless` has an overloaded constructor; the default constructor initializes with all default
settings, such as the built in ExpressionBuilder. You can also pass your own implementation of 
`IExpressionBuilder` for Wayless to use.  If you need to only change a few thigns you 
can inherit from `ExpressionBuilder` and override its implementations.

# WayMore
`WayMore` is the prefered way of getting `Wayless` instances.  It uses Lazy<T> and stores instances in a 
ConcurrentDictionary; Keep in mind that an instance of Wayless is not thread-safe.  

It's best to use the overloaded `Get` method to get instances of Wayless. `Get` checks if 
an instance of the mapper has been cached. If there is one it's re-used, if not, 
a new instance is created and cached.

You can use the overloaded `GetNew` method to get new instance, without caching it, of `Wayless`. 

# Usage

By default `Wayless` will attempt the most basic property name matching by ignoring the casing of each property.
Future releases will allow you to inject your own matching algorithm.

`DontAutoMatchMembers` disables automatic match making.

Example:

	// you can also call GetNew for an uncached instance
	var mapper = WayMore.Get<PerstonDTO, Perston>(); 
	mapper.DontAutoMatchMembers()
	      .FieldMap(dest => dest.Phone
	      	       , src => src.Phone);


Values can be mapped or set using the overloaded `FieldMap` and `FieldSet` methods. If auto matching is enabled 
you can use `FieldSkip` to ignore a field.  

Both `FieldMap` and `FieldSet` have the ability to perform conditional mapping. Meaning, a value will
only  be mapped/set if the supplied condition is met.

	var mapper = WayMore.Mappers.GetNew<PersonDTO, Person>();
	// set phone number to '8675309' if First
	mapper.FieldMap(dest => dest.FirstName
		       , src => src.Nickname
		       , src => src.Phone == "8675309"); 

	var mapper = WayMore.Mappers.GetNew<PersonDTO, Person>();
	// set phone number to '8675309' if First
	mapper.FieldSet(dest => dest.Phone
			      , "8675309"
		       , src => src.FirstName == "Jenny"); 

A call to `Map` applies all the generated rules

# Complex Map
`Wayless` does not currently support complex mappings. To perform a nested complex map you can use `FieldMap`
with a call to `WayMore`

	var mapper = WayMore.Mappers.GetNew<PersonDTO, Person>();
	// For best performance use a cached version of a mapper
	mapper.FieldMap(dest => dest.Sibling
		       , src => WayMore.Mappers
		       		       .Get<PerstonDTO, Person>()
		       		       .Map(src.Sibling)
			); 
	

`Map` has several self-explanatory overloads   that can be used to create a new instance of the specified 
type.
                        
# Performance
Some basic performance tests show that Wayless is nearly as fast as doing manual mappings

A primer call was made for .NET to spin up all its caches. This was also done to let mapping 
APIs build any caches they require

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


Basic object map with no extra conditions; Person -> PersonDTO. 
Set size increase per iteration by 10
    
	------------------------------------
	Primer call

	Manual: 0.3855ms
	AutoMapper: 76.4885ms
	Mapster: 80.5803ms
	Wayless (new instance): 7.4149ms
	Wayless (cached instance): 1.8157ms

	------------------------------------

	Test iteration: 1
	Set size: 10000

	Manual: 0.3828ms
	AutoMapper: 15.0775ms
	Mapster: 0.7932ms
	Wayless (new instance): 0.8922ms
	Wayless (cached instance): 0.3779ms

	------------------------------------

	Test iteration: 2
	Set size: 100000

	Manual: 3.6753ms
	AutoMapper: 22.4071ms
	Mapster: 8.6711ms
	Wayless (new instance): 4.0065ms
	Wayless (cached instance): 3.4095ms

	------------------------------------

	Test iteration: 3
	Set size: 1000000

	Manual: 21.7037ms
	AutoMapper: 219.0175ms
	Mapster: 121.2904ms
	Wayless (new instance): 44.8543ms
	Wayless (cached instance): 38.5314ms

	------------------------------------

	Test iteration: 4
	Set size: 10000000

	Manual: 240.861ms
	AutoMapper: 1931.9526ms
	Mapster: 812.197ms
	Wayless (new instance): 310.1196ms
	Wayless (cached instance): 327.0016ms

	------------------------------------

	Test iteration: 5
	Set size: 100000000

	Manual: 1910.7348ms
	AutoMapper: 18015.3245ms
	Mapster: 8560.8589ms
	Wayless (new instance): 3153.4913ms
	Wayless (cached instance): 3105.1496ms

	------------------------------------


