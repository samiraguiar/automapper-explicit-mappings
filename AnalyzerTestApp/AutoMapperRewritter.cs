using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerTestApp
{
    internal class AutoMapperRewriter : CSharpSyntaxRewriter
    {
        private const int TabSize = 4;
        private static readonly SyntaxTriviaList IdentTrivia =
            SyntaxFactory.TriviaList(Enumerable.Repeat(SyntaxFactory.Space, TabSize));
        private readonly SemanticModel _semanticModel;
        private readonly IDictionary<Tuple<ITypeSymbol, ITypeSymbol>, IList<IPropertySymbol>> _mapping;

        public AutoMapperRewriter(SemanticModel model)
        {
            _semanticModel = model;
            _mapping = new Dictionary<Tuple<ITypeSymbol, ITypeSymbol>, IList<IPropertySymbol>>();
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodSymbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            // symbol could be null, e.g. when invoking a delegate
            if (methodSymbol == null)
            {
                return base.VisitInvocationExpression(node);
            }

            var type = methodSymbol.ContainingType;

            if (methodSymbol.Name == "IgnoreAllNonExisting" && type.Name == "MappingExpressionExtensions")
            {
                var child = node.DescendantNodes().FirstOrDefault(n => n.IsKind(SyntaxKind.InvocationExpression));

                var newChild = Visit(child);

                return newChild.WithoutTrailingTrivia();
            }

            if (type.ContainingSymbol.Name != "AutoMapper")
            {
                return base.VisitInvocationExpression(node);
            }

            // Symbol is a `CreateMap` invocation
            if (MappingDefinitionParser.IsMappingDefinition(methodSymbol, type))
            {
                var mappingDefinitionParser = new MappingDefinitionParser(methodSymbol);
                var mappedTypes = mappingDefinitionParser.GetMappedTypes();

                var sourceMembers = mappingDefinitionParser.GetSourceMembers();
                var destinationMembers = mappingDefinitionParser.GetDestinationMembers();

                var mappedProperties = _mapping.ContainsKey(mappedTypes) ? _mapping[mappedTypes] : new List<IPropertySymbol>();

                var implicitlyMappedProperties = GetImplictlyMappedProperties(destinationMembers, sourceMembers, mappedProperties);
                var ignoredProperties = GetIgnoredProperties(destinationMembers, sourceMembers, mappedProperties);

                var newNode = node;
                var trivia = GetTrivia(node);

                foreach (var implicitlyMappedProperty in implicitlyMappedProperties)
                {
                    var mappingConfigurationGenerator = new MappingConfigurationGenerator(newNode, implicitlyMappedProperty, trivia);
                    newNode = mappingConfigurationGenerator.GetMappingInvocation();
                }

                foreach (var ignoredProperty in ignoredProperties)
                {
                    var mappingConfigurationGenerator = new MappingConfigurationGenerator(newNode, ignoredProperty, trivia);
                    newNode = mappingConfigurationGenerator.GetIgnoreInvocation();
                }

                // The last mapping expression needs to end with newline
                return newNode.WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed));
            }

            if (MappingExpressionParser.IsMappingExpression(methodSymbol, type))
            {
                var mappingExpressionParser = new MappingExpressionParser(node, _semanticModel, methodSymbol);

                var mappedTypes = mappingExpressionParser.GetMappedTypes();

                var destinationProperty = mappingExpressionParser.GetDestinationProperty();
                if (!_mapping.ContainsKey(mappedTypes))
                {
                    _mapping[mappedTypes] = new List<IPropertySymbol>(new[] { destinationProperty });
                }
                else
                {
                    _mapping[mappedTypes].Add(destinationProperty);
                }
            }

            return base.VisitInvocationExpression(node);
        }

        private SyntaxTriviaList GetTrivia(InvocationExpressionSyntax node)
        {
            var existingTrivia = node.GetLeadingTrivia().Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia));
            var leadingTrivia = new SyntaxTriviaList().AddRange(existingTrivia);
            leadingTrivia = leadingTrivia.InsertRange(0, IdentTrivia);
            return leadingTrivia.Insert(0, SyntaxFactory.CarriageReturnLineFeed);
        }

        private IList<string> GetImplictlyMappedProperties(
            IList<IPropertySymbol> allDestinationProperties,
            IList<IPropertySymbol> allSourceProperties,
            IList<IPropertySymbol> mappedProperties)
        {
            var stringSourceProperties = allSourceProperties.Select(x => x.Name);
            var commonProperties = allDestinationProperties.Where(p => stringSourceProperties.Contains(p.Name));

            return commonProperties.Select(p => p.Name)
                    .Except(mappedProperties.Select(p => p.Name)).ToList();
        }

        private IList<string> GetIgnoredProperties(
            IList<IPropertySymbol> allDestinationProperties,
            IList<IPropertySymbol> allSourceProperties,
            IList<IPropertySymbol> mappedProperties)
        {
            var sourcePropertiesNames = allSourceProperties.Select(x => x.Name);
            var onlyOnDestination = allDestinationProperties.Where(x => !sourcePropertiesNames.Contains(x.Name));

            return onlyOnDestination.Select(p => p.Name)
                    .Except(mappedProperties.Select(p => p.Name)).ToList();
        }
    }
}