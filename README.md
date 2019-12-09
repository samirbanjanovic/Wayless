# Wayless

## Benchmarks

``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.0.100
  [Host] : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  Core   : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

Job=Core  Runtime=Core  


|           Method |     Mean |     Error |    StdDev |      Min |      Max | Ratio | RatioSD |
|----------------- |---------:|----------:|----------:|---------:|---------:|------:|--------:|
|       Manual Map | 1.771 us | 0.0048 us | 0.0045 us | 1.765 us | 1.783 us |  1.00 |    0.00 |
|          Wayless | 2.278 us | 0.0055 us | 0.0051 us | 2.272 us | 2.290 us |  1.29 |    0.00 |
|          Mapster | 6.223 us | 0.0546 us | 0.0484 us | 6.123 us | 6.303 us |  3.51 |    0.03 |
```

## Overview

A basic object-object mapper written with speed and usability as the first priority.

To map from a source to destination object create an instance of the Wayless mapper.
By default source and destination properties are matched by name -- case insenstive. 

	var mapper = new Wayless<PersonDTO, Person>(new SetRuleBuilder<PerstonDTO, Person>().UseDefault());

Mapping rules are applied via a call to the `Map` methods.

	PersonDTO personDTO = mapper.Map(person);


## Usage
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

A call to `FinalizeRules()` is __optional__. Wayless will check if the `SetRuleBuilder` has been finalized, if not it will
go ahead and make the call prior to compiling the map function.

## WayMore

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


Once configured you can request and instance of the object pair mapper and then call `Map` through the instance. 

        var personDtoMapper = _waymore.Get<PersonDTO, Person>();
	var personDto = personDtoMapper.Map(person);
	
You can also directly call the `Map` function through `WayMore`.  

        var personDto = _waymore.Map<PersonDTO, Person>(person)
	
Both calls will used cached mapper instances.

## More

You can use `Waymore` with any dependency injection API by registering the `IWayMore` interface. For best performance
you should register it as a singleton with rule declerations. In the example below we use Microsoft's DI API to 
register and configure our singleton 

	services.AddSingleton<IWayMore>
	(
		new WayMore()
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
