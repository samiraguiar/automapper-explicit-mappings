using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerTestApp
{
    internal class AutoMapperRewritter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        public AutoMapperRewritter(SemanticModel model)
        {
            _semanticModel = model;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            // symbol could be null, e.g. when invoking a delegate
            if (symbol == null)
            {
                return base.VisitInvocationExpression(node);
            }

            // symbol must be called Build and have 0 parameters
            if (symbol.Name != "CreateMap" || symbol.Parameters.Length != 0)
            {
                return base.VisitInvocationExpression(node);
            }

            // TODO you might want to check that the parent is not an invocation of .WithOption("addnewline") already

            var type = symbol.ContainingType;

            // symbol must be a method on the type "Mapper.CreateMap"
            if (type.Name != nameof(AutoMapper.Mapper) || type.ContainingSymbol.Name != "AutoMapper")
            {
                return base.VisitInvocationExpression(node);
            }

            var sourceMembers = GetSourceMembers(symbol);
            var destinationMembers = GetDestinationMembers(symbol);

            return null;

            //// TODO you may want to add a check that the containing symbol is a namespace, and that its containing namespace is the global namespace

            //// we have the right one, so return the syntax we want

            //var memberAccessExpression = SyntaxFactory.MemberAccessExpression(
            //            SyntaxKind.SimpleMemberAccessExpression,
            //            node,
            //            SyntaxFactory.IdentifierName("WithOption"));

            //var addNewline = SyntaxFactory.Literal("addnewline");

            //var literalExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, addNewline);

            //var argumentList = SyntaxFactory.ArgumentList(
            //            SyntaxFactory.SingletonSeparatedList(
            //                SyntaxFactory.Argument(literalExpression)));

            //return
            //    SyntaxFactory.InvocationExpression(memberAccessExpression, argumentList);
        }

        private IList<IPropertySymbol> GetSourceMembers(IMethodSymbol methodSymbol)
        {
            return GetMembersFromArgumentType(methodSymbol, 0);
        }

        private IList<IPropertySymbol> GetDestinationMembers(IMethodSymbol methodSymbol)
        {
            return GetMembersFromArgumentType(methodSymbol, 1);
        }

        private IList<IPropertySymbol> GetMembersFromArgumentType(IMethodSymbol methodSymbol, int argumentIndex)
        {
            var namedSourceType = methodSymbol.TypeArguments[argumentIndex];
            var members = namedSourceType.GetMembers().OfType<IPropertySymbol>().ToList();

            return members;
        }
    }
}