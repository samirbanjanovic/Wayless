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
