using AutoMapper;
using Mappers;

namespace AnalyzerTestApp
{
    public class Foo
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public int? CPF { get; set; }
        public Bar OnlyInFoo { get; set; }
    }

    public class Bar
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public int? CPF { get; set; }
        public Foo OnlyInBar { get; set; }
    }

    public class Foo2
    {
        public string Name2 { get; set; }
        public string LastName2 { get; set; }
        public int Age2 { get; set; }
        public int? CPF2 { get; set; }
        public Bar2 OnlyInFoo2 { get; set; }
    }

    public class Bar2
    {
        public string Name2 { get; set; }
        public string Surname2 { get; set; }
        public int Age2 { get; set; }
        public int? CPF2 { get; set; }
        public Foo2 OnlyInBar2 { get; set; }
    }

    public class CustomMapper
    {
        protected void Configure()
        {
            Mapper.CreateMap<Foo, Bar>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(ori => ori.Age))
                .ForMember(dest => dest.CPF, opt => opt.MapFrom(ori => ori.CPF))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName))
                .IgnoreAllNonExisting();

            Mapper.CreateMap<Bar, Foo>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(ori => ori.Age))
                .ForMember(dest => dest.CPF, opt => opt.MapFrom(ori => ori.CPF))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname))
                .IgnoreAllNonExisting();

            Mapper.CreateMap<Bar2, Foo2>()
                .ForMember(dest => dest.Age2, opt => opt.MapFrom(ori => ori.Age2))
                .ForMember(dest => dest.CPF2, opt => opt.MapFrom(ori => ori.CPF2))
                .ForMember(dest => dest.Name2, opt => opt.MapFrom(src => src.Name2))
                .ForMember(dest => dest.LastName2, opt => opt.MapFrom(src => src.Surname2));

            AnotherConfigure();
        }

        private static void AnotherConfigure()
        {
            Mapper.CreateMap<Foo2, Bar2>()
                .ForMember(dest => dest.Age2, opt => opt.MapFrom(ori => ori.Age2))
                .ForMember(dest => dest.CPF2, opt => opt.MapFrom(ori => ori.CPF2))
                .ForMember(dest => dest.Name2, opt => opt.MapFrom(src => src.Name2))
                .ForMember(dest => dest.Surname2, opt => opt.MapFrom(src => src.LastName2));
        }
    }
}
