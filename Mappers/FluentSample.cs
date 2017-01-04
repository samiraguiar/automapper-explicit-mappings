using System.Collections.Generic;

namespace Mappers
{
    public class FluentSample
    {
        private readonly IList<string> _options;

        public FluentSample()
        {
            _options = new List<string>();
        }

        public static FluentSample Build()
        {
            return new FluentSample();
        }

        public FluentSample WithOption(string option)
        {
            _options.Add(option);
            return this;
        }
    }
}
