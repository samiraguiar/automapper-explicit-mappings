using AutoMapper;
using System.Linq;

namespace Mappers
{
    public static class MappingExpressionExtensions
    {
        /// <summary>
        /// Ignora todos os mapeamentos não especificados.
        /// Não afeta os mapeamentos existentes, sejam estes explícitos ou implicitos (propriedade de mesmo nome na 
        /// origem e no destino)
        /// </summary>
        /// <typeparam name="TSource">Origem do mapeamento.</typeparam>
        /// <typeparam name="TDestination">Destino do mapeamento.</typeparam>
        /// <param name="expression">Expressão de mapeamento.</param>
        /// <returns>IMappingExpression&lt;TSource, TDestination&gt;</returns>
        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expression)
        {
            var existingMaps = Mapper.GetAllTypeMaps()
                .FirstOrDefault(x => x.SourceType == typeof(TSource) && x.DestinationType == typeof(TDestination));

            if (existingMaps != null)
            {
                foreach (var property in existingMaps.GetUnmappedPropertyNames())
                {
                    expression.ForMember(property, opt => opt.Ignore());
                }
            }
            return expression;
        }
    }
}
