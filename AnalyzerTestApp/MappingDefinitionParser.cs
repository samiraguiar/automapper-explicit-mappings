using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerTestApp
{
    internal class MappingDefinitionParser
    {
        private readonly IMethodSymbol _methodSymbol;

        public MappingDefinitionParser(IMethodSymbol methodSymbol)
        {
            _methodSymbol = methodSymbol;
        }

        public static bool IsMappingDefinition(IMethodSymbol methodSymbol, INamedTypeSymbol typeSymbol)
        {
            return methodSymbol.Name == nameof(AutoMapper.Mapper.CreateMap) &&
                   methodSymbol.Parameters.Length == 0 &&
                   typeSymbol.Name == nameof(AutoMapper.Mapper);
        }

        public Tuple<ITypeSymbol, ITypeSymbol> GetMappedTypes()
        {
            return Tuple.Create(_methodSymbol.TypeArguments[0], _methodSymbol.TypeArguments[1]);
        }

        public IList<IPropertySymbol> GetSourceMembers()
        {
            return GetMembersFromArgumentType(0);
        }

        public IList<IPropertySymbol> GetDestinationMembers()
        {
            return GetMembersFromArgumentType(1);
        }

        private IList<IPropertySymbol> GetMembersFromArgumentType(int argumentIndex)
        {
            var namedSourceType = _methodSymbol.TypeArguments[argumentIndex];
            var members = namedSourceType.GetMembers().OfType<IPropertySymbol>().ToList();

            return members;
        }
    }
}