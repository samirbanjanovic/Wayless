# Wayless
A lightweight object mapper...less should be more

The idea is to write less mapping code. Using Wayless you can take Object A and request an instance of Object B with field values mapped from A.  By default fields are mapped by name; you can set to ignore casing.

Mappings can be extended by calling the FieldMap and FieldSet methods.  

FieldMap expicitely created a relationship betweeen A.SomeProperty and B.SomeProperty. The idea behind FieldMap is to relate fields that don't share the same name and wouldn't have been linked by default.

var mapper = new WaylessMap<SourceObject, DestinationObject>()
                    .FieldMap(d => d.AssignmentDate, s => s.TimeStamp);

FieldSet allows you to explicitly assign values to a property on the destination object.

var mapper = new WaylessMap<SourceObject, DestinationObject>()
                    .FieldSet(d => d.CorrelationId, Guid.NewGuid());
                    
Both FieldMap and FieldSet use TypeConverter to try and assign values if types don't match.  

Using FieldSkip you can skip value assignment on the destination object.  A call to this method overrides both FieldMap and FieldSet.

var mapper = new WaylessMap<SourceObject, DestinationObject>()
                    .FieldSkip(d => d.CorrelationId);
                    
Think of FieldSet, FieldMap, FieldSkip as rule creations.  None of these rules are applied until the Map() method is called.  On call to Map() al lthe rules are evaluated and applied.

// evalute rules and create instance of new object
var destination = mapper.Map(TestSource);

If you already have a destination object instance adn you want to partially map the rest of it you can call the overloaded Map() method that let's you pass in an existing instance of the destination object.

void Map(TDestination destinationObject); // uses sourceObject passed in to constructor
void Map(TDestination destinationObject, TSource sourceObject);
