# Wayless

A basic object-to-object mapper.
To map from a source to destination object create an instance of the Wayless mapper.
By default the mapper will match source and destination properties by name. 

	var mapper = new Wayless<PersonDTO, Person>(new SetRuleBuilder<PerstonDTO, Person>().UseDefault());

Mapping rules are applied via a call to the `Map` methods.

	PersonDTO personDTO = mapper.Map(person);


# Usage
Unlike prior versions (before 2.0), you have to create an instance of the mapping store, WayMore.

	WayMore _waymore = new WayMore();

From here on you can use your WayMore instance to access and modify mappers. This adds flexibility and doesn't
restrict you to using one set of rules for a pair of mappings. 

By using an instance based store the flow of code is cleaner as well. Instead of calling `WayMore.Wayless.SetRules<TD,TS>` you use 
your instance to `SetRules` - `_wayMoreInstance.SetRules<TD,TS>`. This makes the mappings only valid for that instance, not globally.

Values can be mapped or set using the overloaded `FieldMap` and `FieldSet` methods. If auto matching is enabled 
you can use `FieldSkip` to ignore a field.  

Both `FieldMap` and `FieldSet` have the ability to perform conditional mapping.

	_waymore
	.SetRules<PersonDTO, Person>(cfg =>
	{
		// set phone number to '8675309' if First
		cfg.FieldMap(dest => dest.FirstName
			   , src => src.Nickname
			   , src => src.Phone == "8675309")
		    .FieldSet(dest => dest.Nickname
			    , "Jenny"
			    , src => src.Phone == "8675309")
		    .FinalizeRules(); 
	}

Using a simple Json file you can pair destination and source properties using the `JsonFileMatchMaker`

	_waymore
	.SetRules<PersonDTO, Person>(cfg =>
    {
        cfg.UseJsonMappingMatchMaker(jsonMappingPath)
	       .FinalizeRules(); 
    });

A call to `FinalizeRules()` is optional. Wayless will check if the `SetRuleBuilder` has been finalized, if not it will
go ahead and make the call prior to compiling the map function.

# WayMore

`WayMore` is a thread-safe singleton that stores previously configured and requested instances of `Wayless`.
Via the singleton you can configure and cache mappings for future use by calling `SetRules` or request a mapper by calling the
overloaded `Get` method.

To cache an instance of a mapper you can configure it ahead of time (application startup) and use it 
later by calling the `Get` method, or directly use the mapper by calling the generic `Map` from `WayMore`.

	_waymore
	.SetRules<PersonDTO, Person>(cfg =>
	{
		cfg.FieldMap(d => d.FirstName, s => s.Nickname)
		    .FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
		    .FieldSet(d => d.CreateTime, DateTime.Now)
		    .FinalizeRules(); 
	});

	var personDTO = _waymore.Map<PersonDTO, Person>(person);

`SetRules` returns a reference to `WayMore` to enable chained configurations 

	_waymore
	.SetRules<PersonDTO, Person>(cfg =>
	{
		cfg.FieldMap(d => d.FirstName, s => s.Nickname)
		   .FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
		   .FieldSet(d => d.CreateTime, DateTime.Now)
		   .FinalizeRules(); 
	})
	.SetRules<PersonDTONested, PersonNested>(cfg =>
	{
		var nestedMapper = _waymore.Get<PersonDTO, Person>();
		cfg.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson))
		   .FinalizeRules(); 
	});


To request a cached instance (for repetative calls and better performance) use the overlaoded `Get` method

	var mapper = _waymore.Get<PersonDTONested, PersonNested>();

	mapper.Map(person);


# More

You can use `Waymore` with any dependency injection API by registering the `IWayMore` interface. For best performance
you should register it as a singleton with rule declerations. In the example below we use Microsoft's DI API to 
register and configure our singleton 

	services.AddSingleton<IWayMore>
	(
		new Waymore()
		.SetRules<PersonDTO, Person>(cfg =>
		{
			cfg.FieldMap(d => d.FirstName, s => s.Nickname)
			   .FieldMap(d => d.Nickname, s => $"{s.LastName}, {s.FirstName}")
			   .FieldSet(d => d.CreateTime, DateTime.Now)
			   .FinalizeRules(); 
		})
		.SetRules<PersonDTONested, PersonNested>(cfg =>
		{
			var nestedMapper = waymore.Get<PersonDTO, Person>();
			cfg.FieldMap(x => x.NestedPersonDTO, x => nestedMapper.Map(x.NestedPerson))
			   .FinalizeRules(); 
		})
	);
