# Wayless

Wayless is a basic object-to-object mapper without any fancy features. 
It maps quickly from TypeA to TypeB without special features. As time progresses
more features will be added. However, each addition will be evaluated based on
perfromance. Anything that impacts performance too much will not be included.

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
Some basic performance tests show that Wayless is nearly as fast as doing manual mappings

Initial loop is always slowest due to expression compile

	// Basic Object Map
	// Person -> PersonDTO
	
	Test iteration: 0
	Set size: 1000000
	Manual: 20.1303ms
	AutoMapper Init: 270.6555ms
	AutoMapper: 262.1924ms
	Wayless Init: 3.576ms
	Wayless: 34.2216ms
	Static Wayless: 32.439ms
	Mapster: 198.0777ms
	------------------------------------

	Test iteration: 1
	Set size: 1000000
	Manual: 20.685ms
	AutoMapper Init: 1.8218ms
	AutoMapper: 198.8914ms
	Wayless Init: 0.0419ms
	Wayless: 28.8407ms
	Static Wayless: 28.8675ms
	Mapster: 89.9299ms
	------------------------------------

	Test iteration: 2
	Set size: 1000000
	Manual: 19.8883ms
	AutoMapper Init: 3.0901ms
	AutoMapper: 196.2041ms
	Wayless Init: 0.0713ms
	Wayless: 32.0796ms
	Static Wayless: 31.2334ms
	Mapster: 89.3797ms
	------------------------------------

	Test iteration: 3
	Set size: 1000000
	Manual: 20.219ms
	AutoMapper Init: 1.6156ms
	AutoMapper: 202.6883ms
	Wayless Init: 0.0811ms
	Wayless: 32.6769ms
	Static Wayless: 34.1317ms
	Mapster: 88.2451ms
	------------------------------------

	Test iteration: 4
	Set size: 1000000
	Manual: 19.0561ms
	AutoMapper Init: 1.8003ms
	AutoMapper: 200.7649ms
	Wayless Init: 0.0815ms
	Wayless: 31.4343ms
	Static Wayless: 31.2402ms
	Mapster: 87.061ms
	------------------------------------

	Test iteration: 5
	Set size: 1000000
	Manual: 20.5483ms
	AutoMapper Init: 1.539ms
	AutoMapper: 198.9182ms
	Wayless Init: 0.0547ms
	Wayless: 33.0609ms
	Static Wayless: 32.3084ms
	Mapster: 87.6965ms
	------------------------------------

	Test iteration: 6
	Set size: 1000000
	Manual: 20.8043ms
	AutoMapper Init: 1.5907ms
	AutoMapper: 200.7853ms
	Wayless Init: 0.0407ms
	Wayless: 35.7531ms
	Static Wayless: 29.5094ms
	Mapster: 92.2999ms
	------------------------------------

	Test iteration: 7
	Set size: 1000000
	Manual: 20.5921ms
	AutoMapper Init: 1.6549ms
	AutoMapper: 208.605ms
	Wayless Init: 0.0849ms
	Wayless: 29.708ms
	Static Wayless: 29.5766ms
	Mapster: 87.9861ms
	------------------------------------

	Test iteration: 8
	Set size: 1000000
	Manual: 47.7605ms
	AutoMapper Init: 1.8992ms
	AutoMapper: 200.4353ms
	Wayless Init: 0.057ms
	Wayless: 29.1348ms
	Static Wayless: 29.3652ms
	Mapster: 88.6835ms
	------------------------------------

	Test iteration: 9
	Set size: 1000000
	Manual: 22.9569ms
	AutoMapper Init: 2.219ms
	AutoMapper: 200.8589ms
	Wayless Init: 0.0566ms
	Wayless: 29.759ms
	Static Wayless: 30.0565ms
	Mapster: 87.5926ms
	------------------------------------
