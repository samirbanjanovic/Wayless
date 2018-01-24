# Wayless

`var mapper = new WaylessMap<SourceType, DestinationType>();`

The constructor has an optional parameter to ignore case when matching property names. 
The evaluation does nothing sophisticated; it looks for a 1-1 match. 

Default rules can be extended using `FieldMap`, `FiledSet`, and `FieldSkip` methods. 

`FieldMap`: create an explicit mapping relationship between properties in your destination
and source type.

`FieldSet`: set a value explicitly. 

`FieldSkip`: removes property in destination type from mapping rules. Calling this method will override 
any rules you created using `FieldMap` and `FieldSet`.

`FieldRestore`: restores a previously skipped field to be mapped again

    var TestSource = new SourceObject()
    {
        Id = 1,
        InstanceName = "Source",
        TimeStamp = DateTime.Now.AddDays(-1)
    };

    var mapperInstance = new WaylessMap<DestinationObject, SourceObject>()
            .FieldMap(dest => dest.AssignmentDate, src => src.TimeStamp)
            .FieldMap(dest => d.Name, src => src.InstanceName)
            .FieldSet(dest => dest.CorrelationId, Guid.NewGuid())
            .FieldSkip(dest => dest.ClosingDate);
                        
    var mappedObject = mapperInstance                        
            .Map(SourceObject);

A call to `Map` applies all the generated rules and generates an instance of the submitted type.

`Map` has several self-explanatory overloads   that can be used to create a new instance of the specified 
type.

If you want to reuse the mapper and you've applied a `FieldSkip` you can  call `FieldRestore` to start mapping the field again.

    var mappedObject = mapperInstance                        
            .FieldRestore(dest => dest.ClosingDate)
            .Map(SourceObject);
                        
# Perfromance
Some basic performance tests show that Wayless works best on small number of iterations

Initial loop is always the slowest due to .NET caching and evaluations

    Test iteration: 0
    Set size: 1000

    Manual: 0.3673ms
    AutoMapper Init: 273.9257ms
    AutoMapper: 66.4153ms
    Wayless Init: 5.3337ms
    Wayless: 2.1238ms
    ----------------------------
    Test iteration: 1
    Set size: 1000

    Manual: 0.0487ms
    AutoMapper Init: 1.6632ms
    AutoMapper: 3.5311ms
    Wayless Init: 1.5696ms
    Wayless: 0.4191ms
    ----------------------------
    Test iteration: 2
    Set size: 1000

    Manual: 0.0645ms
    AutoMapper Init: 1.3777ms
    AutoMapper: 4.0869ms
    Wayless Init: 1.2078ms
    Wayless: 0.3885ms
    ----------------------------
    Test iteration: 3
    Set size: 1000

    Manual: 0.0717ms
    AutoMapper Init: 1.477ms
    AutoMapper: 4.0201ms
    Wayless Init: 1.7602ms
    Wayless: 0.6834ms
    ----------------------------
    Test iteration: 4
    Set size: 1000

    Manual: 0.0906ms
    AutoMapper Init: 10.8361ms
    AutoMapper: 3.9974ms
    Wayless Init: 1.2097ms
    Wayless: 0.3749ms
    ----------------------------
    Test iteration: 5
    Set size: 1000

    Manual: 0.0445ms
    AutoMapper Init: 1.4234ms
    AutoMapper: 3.7565ms
    Wayless Init: 1.3373ms
    Wayless: 0.4814ms
    ----------------------------
    Test iteration: 6
    Set size: 1000

    Manual: 0.0249ms
    AutoMapper Init: 1.1757ms
    AutoMapper: 5.2604ms
    Wayless Init: 1.4891ms
    Wayless: 0.7317ms
    ----------------------------
    Test iteration: 7
    Set size: 1000

    Manual: 0.0252ms
    AutoMapper Init: 1.2433ms
    AutoMapper: 3.872ms
    Wayless Init: 1.4823ms
    Wayless: 0.4644ms
    ----------------------------
    Test iteration: 8
    Set size: 1000

    Manual: 0.049ms
    AutoMapper Init: 1.2222ms
    AutoMapper: 3.6848ms
    Wayless Init: 1.2361ms
    Wayless: 0.5388ms
    ----------------------------
    Test iteration: 9
    Set size: 1000

    Manual: 0.043ms
    AutoMapper Init: 2.7348ms
    AutoMapper: 4.941ms
    Wayless Init: 1.5076ms
    Wayless: 0.5067ms
    ----------------------------
    Test iteration: 10
    Set size: 1000

    Manual: 0.026ms
    AutoMapper Init: 1.2456ms
    AutoMapper: 3.9408ms
    Wayless Init: 1.397ms
    Wayless: 0.5516ms
