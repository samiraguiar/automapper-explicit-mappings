using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzerTestApp
{
    internal class ExpressionRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;
        public IPropertySymbol PropertySymbol;

        public ExpressionRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            PropertySymbol = _semanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
            return base.VisitMemberAccessExpression(node);
        }
    }
}