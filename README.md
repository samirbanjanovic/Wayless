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
	Manual: 93.3964ms
	AutoMapper Init: 252.3398ms
	AutoMapper: 379.3795ms
	Wayless Init: 4.8602ms
	Wayless: 128.9974ms
	Static Wayless: 126.7195ms
	Mapster: 274.1859ms
	------------------------------------

	Test iteration: 1
	Set size: 1000000
	Manual: 82.4058ms
	AutoMapper Init: 1.6874ms
	AutoMapper: 261.6944ms
	Wayless Init: 0.0415ms
	Wayless: 124.4917ms
	Static Wayless: 120.6861ms
	Mapster: 151.7682ms
	------------------------------------

	Test iteration: 2
	Set size: 1000000
	Manual: 83.2357ms
	AutoMapper Init: 1.3781ms
	AutoMapper: 265.9154ms
	Wayless Init: 0.0634ms
	Wayless: 109.0895ms
	Static Wayless: 114.0573ms
	Mapster: 154.1832ms
	------------------------------------

	Test iteration: 3
	Set size: 1000000
	Manual: 87.1803ms
	AutoMapper Init: 1.8342ms
	AutoMapper: 269.5937ms
	Wayless Init: 0.0709ms
	Wayless: 106.7016ms
	Static Wayless: 116.1053ms
	Mapster: 152.6087ms
	------------------------------------

	Test iteration: 4
	Set size: 1000000
	Manual: 89.3276ms
	AutoMapper Init: 2.2439ms
	AutoMapper: 262.5821ms
	Wayless Init: 0.0385ms
	Wayless: 106.5721ms
	Static Wayless: 108.3256ms
	Mapster: 153.471ms
	------------------------------------

	Test iteration: 5
	Set size: 1000000
	Manual: 85.2894ms
	AutoMapper Init: 1.7017ms
	AutoMapper: 260.9426ms
	Wayless Init: 0.0362ms
	Wayless: 105.49ms
	Static Wayless: 105.891ms
	Mapster: 158.5295ms
	------------------------------------

	Test iteration: 6
	Set size: 1000000
	Manual: 82.5636ms
	AutoMapper Init: 2.481ms
	AutoMapper: 269.963ms
	Wayless Init: 0.0558ms
	Wayless: 129.0431ms
	Static Wayless: 114.1468ms
	Mapster: 157.9677ms
	------------------------------------

	Test iteration: 7
	Set size: 1000000
	Manual: 85.767ms
	AutoMapper Init: 1.3245ms
	AutoMapper: 267.0405ms
	Wayless Init: 0.0743ms
	Wayless: 128.2385ms
	Static Wayless: 106.1439ms
	Mapster: 152.5728ms
	------------------------------------

	Test iteration: 8
	Set size: 1000000
	Manual: 83.2293ms
	AutoMapper Init: 1.6111ms
	AutoMapper: 274.7889ms
	Wayless Init: 0.0925ms
	Wayless: 124.2316ms
	Static Wayless: 109.8348ms
	Mapster: 154.9066ms
	------------------------------------

	Test iteration: 9
	Set size: 1000000
	Manual: 84.0238ms
	AutoMapper Init: 1.7795ms
	AutoMapper: 264.7418ms
	Wayless Init: 0.0804ms
	Wayless: 130.2362ms
	Static Wayless: 106.2901ms
	Mapster: 160.0934ms
	------------------------------------
