using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace AnalyzerTestApp
{
    internal class MappingExpressionParser
    {
        private readonly IMethodSymbol _methodSymbol;
        private readonly InvocationExpressionSyntax _node;
        private readonly SemanticModel _semanticModel;

        public MappingExpressionParser(InvocationExpressionSyntax node, SemanticModel semanticModel, IMethodSymbol methodSymbol)
        {
            _node = node;
            _semanticModel = semanticModel;
            _methodSymbol = methodSymbol;
        }

        public static bool IsMappingExpression(IMethodSymbol methodSymbol, INamedTypeSymbol typeSymbol)
        {
            return methodSymbol.Name == nameof(AutoMapper.IMappingExpression.ForMember) &&
                   methodSymbol.Parameters.Length == 2 &&
                   typeSymbol.Name == nameof(AutoMapper.IMappingExpression);
        }

        public Tuple<ITypeSymbol, ITypeSymbol> GetMappedTypes()
        {
            var returnType = _methodSymbol.ReturnType as INamedTypeSymbol;
            return returnType != null
                              ? Tuple.Create(returnType.TypeArguments[0], returnType.TypeArguments[1])
                              : null;
        }

        public IPropertySymbol GetDestinationProperty()
        {
            var expression = _node.ArgumentList.Arguments[0].Expression;

            var expressionRewriter = new ExpressionRewriter(_semanticModel);
            expressionRewriter.Visit(expression);

            return expressionRewriter.PropertySymbol;
        }
    }
}