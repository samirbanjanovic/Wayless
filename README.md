# Wayless

A basic object-to-object mapper.
To map from a source to destination object create an instance of the Wayless mapper.
By default the mapper will match source and destination properties by name. 

	var mapper = new Wayless<PersonDTO, Person>();

Mapping rules are applied via a call to the `Map` methods.

	PersonDTO personDTO = mapper.Map(person);

# Usage

Values can be mapped or set using the overloaded `FieldMap` and `FieldSet` methods. If auto matching is enabled 
you can use `FieldSkip` to ignore a field.  

Both `FieldMap` and `FieldSet` have the ability to perform conditional mapping.


	WayMore
	.Wayless
	.SetRules<PersonDTO, Person>(cfg =>
	{
		// set phone number to '8675309' if First
		cfg.FieldMap(dest => dest.FirstName
			   , src => src.Nickname
			   , src => src.Phone == "8675309"); 						
	}

	WayMore
	.Wayless
	.SetRules<PersonDTO, Person>(cfg =>
	{
		// set phone number to '8675309' if First
		cfg.FieldSet(dest => dest.FirstName
			    , "Jenny"
			    , src => src.Phone == "8675309"); 
	}

# WayMore

`WayMore` is a thread-safe singleton that stores previously configured and requested instances of `Wayless`.
Via the singleton you can configure and cache mappings for future use by calling `SetRules` or request a mapper by calling the
overloaded `Get` method.

To cache an instance of a mapper you can configure it ahead of time (application startup) and use it 
later by calling the `Get` method, or directly use the mapper by calling the generic `Map` from `WayMore`.

	WayMore
	.Wayless
	.SetRules<PersonDTO, Person>(cfg =>
	{
		cfg.FieldMap(d => d.FirstName, s => s.Nickname)
		    .FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
		    .FieldSet(d => d.CreateTime, DateTime.Now);
	});

	var personDTO = WayMore.Wayless.Map<PersonDTO, Person>(person);

`SetRules` returns a reference to `WayMore` to enable chained configurations 

	WayMore
	.Wayless
	.SetRules<PersonDTO, Person>(cfg =>
	{
		cfg.FieldMap(d => d.FirstName, s => s.Nickname)
		   .FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
		   .FieldSet(d => d.CreateTime, DateTime.Now);
	})
	.SetRules<StudentDTO, Student>(cfg =>
	{
		cfg.FieldSet(d => d.RegisterDate, DateTime.Now)
		   .FieldMap(d => d.DOB, s => s.DateOfBirth);
	});


To request a cached instance (for repetative calls and better performance) use the overlaoded `Get` method

	var mapper = WayMore
		     .Wayless
		     .Get<PersonDTONested, PersonNested>();

Once you have a mapper instance you can further set rules or call the map function

	mapper.FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
	      .FieldSet(d => d.CreateTime, DateTime.Now);

	mapper.Map(person);



# Complex Map
`Wayless` currently does not support complex mappings directly. To perform a nested complex map you can use `FieldMap`
with a call to `WayMore`. To achieve better performance assign the nested mapper and pass the reference, instead of 
passing the call to `WayMore`

	var nestedMapper = WayMore.Wayless.Get<PersonDTO, Person>();
	var mapper = WayMore.Wayless
			.Get<PersonDTONested, PersonNested>();

	mapper.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson));
	var personDtoNested = mapper.Map(personNested);
	
#More

You can use waymore with any dependency injection API by registering the `IWayMore` interface as a singleton 
implemented through `WayMore.Wayless`. If using ASP.NET Core this enables you to configure all your mappers in your `Startup.cs`

	services.AddSingleton<IWayMore>(x => WayMore.Wayless);

Once registered you can change your `Configure` method to expect `IWayMore`. 


	public void Configure(IApplicationBuilder app, IHostingEnvironment env, IWayMore waymore)
	{
		waymore
		.SetRules<PersonDTO, Person>(cfg =>
		{
			cfg.FieldMap(d => d.FirstName, s => s.Nickname)
			   .FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
			   .FieldSet(d => d.CreateTime, DateTime.Now);
		})
		.SetRules<PersonDTONested, PersonNested>(cfg =>
		{
			var nestedMapper = WayMore.Wayless.Get<PersonDTO, Person>();
			cfg.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson));
		});
		
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseBrowserLink();
		}
		else
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseStaticFiles();
		app.UseMvc();
	}

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


