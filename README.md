# Wayless
Wayless came about when other developers I work with needed a quick and easy 
way to map from our database entities to application models.  
We started out with Automapper but quickly realized it was a little much for our needs. 
It introduced complexity in several areas where we felt it wasn't needed.  We wanted 
to create a quick little class that can go from Entity-To-Model without much fuss.  
That's how the first iteration of Wayless came to be; it was a simple Object-To-Object mapper
that used Attributes to create mapping relationships; it was written in classic .NET. 
With us moving to .NET Core we created new standards where our shared libraries had 
to conform to .NET Standard 2.0. Thus, we rewrote our little mapper.
The first design choice we made was to move away from using attributes to establish 
relationships. We felt that sprinkling attributes all over our Model objects made for some 
gross code. That's how our current iteation of the Object-To-Object mapper started.

Internally it's nothing fancy, heck, it might be event poor design.  It uses dictionaries and
reflection to create an initial set of mapping rules based on property name matching.  

Creating an instance of `WaylessMap` involves setting a `Destination` and `Source` type the 
mapper can use to build its initial relationships.

    var mapper = new WaylessMap<SourceType, DestinationType>();

The constructor has an optional parameter to ignore case when matching property names. 
The evaluation does nothing sophisticated, it looks for matching property names to 
create relationships. The constructor also has an overload to take in
one `SourceObject` that will be used when the appropriate `Map` overload is called.

Default rules can be extended and modified using the `FieldMap`, `FiledSet`, and `FieldSkip` methods. 

`FieldMap`: create an explicit mapping relationship between properties in your destination
and source type.

`FieldSet`: set a value explicitly. 

`FieldSkip`: removes property in destination type from mapping rules. Calling this method will override 
any rules you created using `FieldMap` and `FieldSet`.

    var mappedObject = new WaylessMap<DestinationType, SourceType>()
                        .FieldMap(d => d.AssignmentDate, s => s.TimeStamp)
                        .FieldMap(d => d.Name, d => InstanceName)
                        .FieldSet(d => d.CorrelationId, Guid.NewGuid())
                        .FieldSkip(d => d.ClosingDate)
                        .Map(SourceObject);

When `Map` is called all the rules created using default analsys, `FieldMap`, `FieldSet`, and `FieldSkip` 
are evaluated and applied.  
`Map` has several self-explanatory overloads   that can be used to create a new instance of the specified 
type or it can set values of an existing instance.

You can review the relationships that were created between two types by calling the `ShowMapping` method.
This will return a simple text string identifying all the generated mapping rules
    
    DestinationType.DestinationProperty = SourceType.SourceProperty
    DestinationType.DestinationProperty2 = "SomeExplicitValue"
    DestinationType.DestinationProperty3 - Skip
    

That's all there's to it. Just a simple Object-To-Object mapper.
