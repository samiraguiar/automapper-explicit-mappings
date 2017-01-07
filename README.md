# automapper-explicit-mappings
### Fix code that implicitly maps properties or use IgnoreAllNonExisting

AutoMapper has a built-in test to make sure that all properties, usually from the destination, are explicitly mapped (`Mapper.AssertConfigurationIsValid`).
Many projects misuse AutoMapper and use a famous extension method that automatically ignores all unmapped properties to make that test pass.
It's also common for them to take advantage of AutoMapper's ability to auto mapping (no pun intended) properties with the same name and leave them out of the mapping convention.

Those two common actions make it harder to spot errors and to refactor code. For instance, when renaming a property R# won't know whether it was mapped implictly or ignored (via `IgnoreAllNonExisting`) by AutoMapper.

This code attempts to fix this situation by doing three things:
- It explicitly maps all properties with the same name in both source and destination;
- It explicitly ignores all properties that are present in the destination but not in the source. This is basically what `IgnoreAllNonExisting` does under the covers;
- It removes all the `IgnoreAllNonExisting` calls.

With those steps taken, it becomes easier to refactor code and perhaps even remove AutoMapper. The `AssertConfigurationIsValid` should correctly work afterwards.

### Code
The code depends on the new Roslyn API and is very poorly written.

This was a quick and dirty solution to a private company's problem that needed to be addressed ASAP, so no care was put into covering all edges cases, applying design patterns or adding tests. Besides, there was no previous knowledge of Roslyn, so many things might seem a bit weird and/or over complicated to some people.

### Caveats
As stated above, this code doesn't cover all edges cases. Essentially it turns this:

```csharp
Mapper.CreateMap<Foo, Bar>()
    .ForMember(src => src.Name, opt => opt.MapFrom(dest => dest.Nickname))
    .ForMember(src => src.Age, opt => opt.MapFrom(dest => dest.Years))
    .IgnoreAllNonExisting();
```

into this:

```csharp
Mapper.CreateMap<Foo, Bar>()
    .ForMember(src => src.Number, opt => opt.MapFrom(dest => dest.Number))
    .ForMember(src => src.Surname, opt => opt.Ignore())
    .ForMember(src => src.Name, opt => opt.MapFrom(dest => dest.Nickname))
    .ForMember(src => src.Age, opt => opt.MapFrom(dest => dest.Years));
```

Some of the caveats with this approach are:
- It always expects the mappings to be written in fluent style and in the same statement;
- It expects the `IgnoreAllNonExisting` calls to be at the end of the statement;
- It doesn't cover `.ForAllMembers` and the other `IMappingExpression` methods;

...

Nevertheless, hopefully this may help someone to get started with refactoring AutoMapper's mapping configurations or removing them altogether.

**Note:** this repository has a copy of the aforementioned `IgnoreAllNonExisting` method. No credit was given as the original source was not found, but the credits will be added if the author so desires. 
