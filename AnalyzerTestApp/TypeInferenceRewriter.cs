using Mappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzerTestApp
{
    public class TypeInferenceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel SemanticModel;

        public TypeInferenceRewriter(SemanticModel semanticModel)
        {
            SemanticModel = semanticModel;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            // symbol could be null, e.g. when invoking a delegate
            if (symbol == null)
            {
                return base.VisitInvocationExpression(node);
            }

            // symbol must be called Build and have 0 parameters
            if (symbol.Name != "Build" ||
                symbol.Parameters.Length != 0)
            {
                return base.VisitInvocationExpression(node);
            }

            // TODO you might want to check that the parent is not an invocation of .WithOption("addnewline") already

            // symbol must be a method on the type "Fluent.FluentSample"
            var type = symbol.ContainingType;

            if (type.Name != nameof(FluentSample) || type.ContainingSymbol.Name != "Mappers")
            {
                return base.VisitInvocationExpression(node);
            }

            // TODO you may want to add a check that the containing symbol is a namespace, and that its containing namespace is the global namespace

            // we have the right one, so return the syntax we want

            var memberAccessExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        node,
                        SyntaxFactory.IdentifierName("WithOption"));

            var addNewline = SyntaxFactory.Literal("addnewline");

            var literalExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, addNewline);

            var argumentList = SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(literalExpression)));

            return
                SyntaxFactory.InvocationExpression(memberAccessExpression, argumentList);

        }
    }
}
