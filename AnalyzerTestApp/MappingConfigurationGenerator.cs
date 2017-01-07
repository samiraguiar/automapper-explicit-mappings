using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzerTestApp
{
    internal class MappingConfigurationGenerator
    {
        private readonly SyntaxTriviaList _trailingTrivia;
        private readonly InvocationExpressionSyntax _node;
        private readonly string _mappedMemberName;

        private const string DestinationParameterName = "dest";
        private const string SourceParameterName = "ori";
        private const string OptionsParameterName = "opt";

        private static readonly SyntaxTriviaList SingleSpace = SyntaxFactory.TriviaList(SyntaxFactory.Space);
        // gen: ` => `
        private static readonly SyntaxToken ArrowToken = SyntaxFactory.Token(
                                                              SingleSpace,
                                                              SyntaxKind.EqualsGreaterThanToken,
                                                              SingleSpace
                                                         );

        public MappingConfigurationGenerator(InvocationExpressionSyntax node, string mappedMemberName, SyntaxTriviaList trailingTrivia)
        {
            _node = node;
            _mappedMemberName = mappedMemberName;
            _trailingTrivia = trailingTrivia;
        }

        public InvocationExpressionSyntax GetIgnoreInvocation()
        {
            var destinationLambda = GetLambda(DestinationParameterName, _mappedMemberName);

            // gen: `{_optionsParameterName}`
            var parameter = SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(
                                    SyntaxFactory.TriviaList(),
                                    OptionsParameterName,
                                    SyntaxFactory.TriviaList())
                            );

            // gen: `{_optionsParameterName}.Ignore()`
            var invocationExpression = SyntaxFactory.InvocationExpression(
                                          SyntaxFactory.MemberAccessExpression(
                                              SyntaxKind.SimpleMemberAccessExpression,
                                              SyntaxFactory.IdentifierName(OptionsParameterName),
                                              SyntaxFactory.IdentifierName("Ignore")
                                          )
                                       );

            // gen: `{_optionsParameterName} => {_optionsParameterName}.MapFrom({_sourceParameterName} => {_sourceParameterName}.{_mappedMemberName})`
            var ignoreLambda = SyntaxFactory.SimpleLambdaExpression(parameter, invocationExpression)
                                            .WithArrowToken(ArrowToken);

            return GetForMemberInvocation(ignoreLambda);
        }

        public InvocationExpressionSyntax GetMappingInvocation()
        {
            var sourceLambda = GetOptionsLambda();
            return GetForMemberInvocation(sourceLambda);
        }

        public InvocationExpressionSyntax GetForMemberInvocation(SimpleLambdaExpressionSyntax sourceLambda)
        {
            var destinationLambda = GetLambda(DestinationParameterName, _mappedMemberName);

            var invocationExpression = SyntaxFactory.InvocationExpression(
                                          SyntaxFactory.MemberAccessExpression(
                                              SyntaxKind.SimpleMemberAccessExpression,
                                              _node.WithTrailingTrivia(_trailingTrivia),
                                              SyntaxFactory.IdentifierName("ForMember")
                                          )
                                       );

            var commaToken = SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(),
                                SyntaxKind.CommaToken,
                                SyntaxFactory.TriviaList(SingleSpace));
            var arguments = SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[] {
                                        SyntaxFactory.Argument(destinationLambda),
                                        commaToken,
                                        SyntaxFactory.Argument(sourceLambda)
                                    }
                                )
                            );

            // gen: `.ForMember({destinationLambda, sourceLambda})
            invocationExpression = invocationExpression.WithArgumentList(arguments);

            return invocationExpression;
        }

        private SimpleLambdaExpressionSyntax GetLambda(string parameterName, string memberName)
        {
            // gen: `{parameterName}`
            var parameter = SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(
                                    SyntaxFactory.TriviaList(),
                                    parameterName,
                                    SyntaxFactory.TriviaList())
                            );

            // gen: `{parameterName}.{memberName}`
            var memberAccess = SyntaxFactory.MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   SyntaxFactory.IdentifierName(parameterName),
                                   SyntaxFactory.IdentifierName(memberName)
                               );

            // gen: `{parameterName} => {parameterName}.{memberName}`
            return SyntaxFactory.SimpleLambdaExpression(parameter, memberAccess)
                                .WithArrowToken(ArrowToken);
        }

        private SimpleLambdaExpressionSyntax GetOptionsLambda()
        {
            // gen: `{_sourceParameterName} => {_sourceParameterName}.{_mappedMemberName}`
            var sourceLambda = GetLambda(SourceParameterName, _mappedMemberName);
            var sourceLambdaAsArgument = SyntaxFactory.ArgumentList(
                                             SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(sourceLambda)
                                             )
                                         );

            // gen: `{_optionsParameterName}`
            var parameter = SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier(
                                    SyntaxFactory.TriviaList(),
                                    OptionsParameterName,
                                    SyntaxFactory.TriviaList())
                            );

            // gen: `{_optionsParameterName}.MapFrom()`
            var invocationExpression = SyntaxFactory.InvocationExpression(
                                          SyntaxFactory.MemberAccessExpression(
                                              SyntaxKind.SimpleMemberAccessExpression,
                                              SyntaxFactory.IdentifierName(OptionsParameterName),
                                              SyntaxFactory.IdentifierName("MapFrom")
                                          )
                                       );

            // gen: `{_optionsParameterName}.MapFrom({_sourceParameterName} => {_sourceParameterName}.{_mappedMemberName})`
            invocationExpression = invocationExpression.WithArgumentList(sourceLambdaAsArgument);

            // gen: `{_optionsParameterName} => {_optionsParameterName}.MapFrom({_sourceParameterName} => {_sourceParameterName}.{_mappedMemberName})`
            return SyntaxFactory.SimpleLambdaExpression(parameter, invocationExpression)
                                .WithArrowToken(ArrowToken);
        }
    }
}
