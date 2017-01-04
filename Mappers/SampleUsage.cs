using Mappers;

namespace AnalyzerTestApp
{
    public class FooSample
    {
        public void Build()
        {
            FluentSample.Build()
                .WithOption("addnewline")
                .WithOption("uppercase")
                .WithOption("trim")
                .WithOption("concat");
        }
    }
}
