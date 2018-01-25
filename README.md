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
                        
# Perfromance
Some basic performance tests show that Wayless works best on a small number of iterations. It's able to maintain
a performance edge up to ~20,000 iterations

Initial loop is always the slowest due to .NET caching and evaluations

    Test iteration: 0
    Set size: 1000
    Manual: 0.2708ms
    AutoMapper Init: 191.2295ms
    AutoMapper: 50.1818ms
    Wayless Init: 4.8484ms
    Wayless: 1.2306ms
    Static Wayless: 1.1678ms
    Mapster: 62.9199ms
    ------------------------------------

    Test iteration: 1
    Set size: 1000
    Manual: 0.0449ms
    AutoMapper Init: 1.3316ms
    AutoMapper: 3.0857ms
    Wayless Init: 0.9938ms
    Wayless: 0.1509ms
    Static Wayless: 0.1489ms
    Mapster: 0.0942ms
    ------------------------------------

    Test iteration: 2
    Set size: 1000
    Manual: 0.0472ms
    AutoMapper Init: 1.238ms
    AutoMapper: 3.1361ms
    Wayless Init: 1.0638ms
    Wayless: 0.1466ms
    Static Wayless: 0.1443ms
    Mapster: 0.0922ms
    ------------------------------------

    Test iteration: 3
    Set size: 1000
    Manual: 0.0457ms
    AutoMapper Init: 3.2357ms
    AutoMapper: 3.0829ms
    Wayless Init: 1.0081ms
    Wayless: 0.118ms
    Static Wayless: 0.1144ms
    Mapster: 0.0654ms
    ------------------------------------

    Test iteration: 4
    Set size: 1000
    Manual: 0.0212ms
    AutoMapper Init: 0.9706ms
    AutoMapper: 3.0459ms
    Wayless Init: 0.9665ms
    Wayless: 0.1134ms
    Static Wayless: 0.1126ms
    Mapster: 0.0648ms
    ------------------------------------

    Test iteration: 5
    Set size: 1000
    Manual: 0.0206ms
    AutoMapper Init: 0.9378ms
    AutoMapper: 3.0477ms
    Wayless Init: 0.9734ms
    Wayless: 0.1142ms
    Static Wayless: 0.1121ms
    Mapster: 0.0666ms
    ------------------------------------

    Test iteration: 6
    Set size: 1000
    Manual: 0.0199ms
    AutoMapper Init: 0.9353ms
    AutoMapper: 3.0536ms
    Wayless Init: 1.0086ms
    Wayless: 0.1481ms
    Static Wayless: 0.1438ms
    Mapster: 0.5912ms
    ------------------------------------

    Test iteration: 7
    Set size: 1000
    Manual: 0.0173ms
    AutoMapper Init: 1.2513ms
    AutoMapper: 3.1637ms
    Wayless Init: 1.1157ms
    Wayless: 0.131ms
    Static Wayless: 0.1223ms
    Mapster: 0.0679ms
    ------------------------------------

    Test iteration: 8
    Set size: 1000
    Manual: 0.0232ms
    AutoMapper Init: 0.9363ms
    AutoMapper: 3.1302ms
    Wayless Init: 1.0784ms
    Wayless: 0.1162ms
    Static Wayless: 0.1223ms
    Mapster: 0.07ms
    ------------------------------------

    Test iteration: 9
    Set size: 1000
    Manual: 0.0219ms
    AutoMapper Init: 0.9353ms
    AutoMapper: 3.1225ms
    Wayless Init: 1.0779ms
    Wayless: 0.119ms
    Static Wayless: 0.1221ms
    Mapster: 0.0687ms
    ------------------------------------

    Test iteration: 10
    Set size: 1000
    Manual: 0.0227ms
    AutoMapper Init: 0.932ms
    AutoMapper: 3.0186ms
    Wayless Init: 0.9808ms
    Wayless: 0.3073ms
    Static Wayless: 0.1216ms
    Mapster: 0.0702ms
    ------------------------------------
