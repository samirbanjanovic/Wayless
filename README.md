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

    var TestSource = new SourceObject()
    {
        Id = 1,
        InstanceName = "Source",
        TimeStamp = DateTime.Now.AddDays(-1)
    };

    var mappedObject = new WaylessMap<DestinationObject, SourceObject>()
                        .FieldMap(dest => dest.AssignmentDate, src => src.TimeStamp)
                        .FieldMap(dest => d.Name, src => src.InstanceName)
                        .FieldSet(dest => dest.CorrelationId, Guid.NewGuid())
                        .FieldSkip(dest => dest.ClosingDate)
                        .Map(SourceObject);

A call to `Map` applies all the generated rules and generates an instance of the submitted type.

`Map` has several self-explanatory overloads   that can be used to create a new instance of the specified 
type.

You can review the relationships that were created between two types by calling the `ShowMapping` method.
This will return a simple text string identifying all the generated mapping rules
    
    DestinationType.DestinationProperty = SourceType.SourceProperty
    DestinationType.DestinationProperty2 = "SomeExplicitValue"
    DestinationType.DestinationProperty3 - Skip   
