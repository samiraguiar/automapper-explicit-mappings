using AutoMapper;

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

    public class CustomMapper
    {
        protected void Configure()
        {
            Mapper.CreateMap<Foo, Bar>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(ori => ori.Age))
                .ForMember(dest => dest.CPF, opt => opt.MapFrom(ori => ori.CPF))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName));

            Mapper.CreateMap<Bar, Foo>()
    
            .ForMember(dest => dest.Age, opt => opt.MapFrom(ori => ori.Age))
    
            .ForMember(dest => dest.CPF, opt => opt.MapFrom(ori => ori.CPF))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname));
        }
    }
}
