// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiConventionAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode,
            DiagnosticDescriptors.MVC1005_ActionReturnsUndocumentedSuccessResult,
            DiagnosticDescriptors.MVC1006_ActionDoesNotReturnDocumentedStatusCode);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var symbolCache = new ApiControllerSymbolCache(compilationStartAnalysisContext.Compilation);
                if (symbolCache.ApiConventionTypeAttribute == null || symbolCache.ApiConventionTypeAttribute.TypeKind == TypeKind.Error)
                {
                    // No-op if we can't find types we care about.
                    return;
                }

                InitializeWorker(compilationStartAnalysisContext, symbolCache);
            });
        }

        private void InitializeWorker(CompilationStartAnalysisContext compilationStartAnalysisContext, ApiControllerSymbolCache symbolCache)
        {
            compilationStartAnalysisContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
            {
                var methodSyntax = (MethodDeclarationSyntax)syntaxNodeContext.Node;
                var semanticModel = syntaxNodeContext.SemanticModel;
                var method = semanticModel.GetDeclaredSymbol(methodSyntax, syntaxNodeContext.CancellationToken);

                var conventionAttributes = GetConventionTypeAttributes(symbolCache, method);

                if (!ShouldEvaluateMethod(symbolCache, method, conventionAttributes))
                {
                    return;
                }

                var returnType = UnwrapMethodReturnType(symbolCache, semanticModel, method, methodSyntax);

                var expectedResponseMetadata = SymbolApiResponseMetadataProvider.GetResponseMetadata(symbolCache, method, conventionAttributes);
                var actualResponseMetadata = new HashSet<int>();

                var context = new ApiConventionContext(
                    symbolCache, 
                    syntaxNodeContext, 
                    expectedResponseMetadata, 
                    actualResponseMetadata, 
                    returnType);

                foreach (var returnStatementSyntax in methodSyntax.DescendantNodes().OfType<ReturnStatementSyntax>())
                {
                    VisitReturnStatementSyntax(context, returnStatementSyntax);
                }

                for (var i = 0; i < expectedResponseMetadata.Count; i++)
                {
                    var expectedStatusCode = expectedResponseMetadata[i].StatusCode;
                    if (!actualResponseMetadata.Contains(expectedStatusCode))
                    {
                        context.SyntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.MVC1006_ActionDoesNotReturnDocumentedStatusCode,
                            methodSyntax.Identifier.GetLocation()));
                    }
                }

            }, SyntaxKind.MethodDeclaration);
            
        }

        private IReadOnlyList<AttributeData> GetConventionTypeAttributes(ApiControllerSymbolCache symbolCache, IMethodSymbol method)
        {
            var attributes = method.ContainingType.GetAttributes(symbolCache.ApiConventionTypeAttribute).ToArray();
            if (attributes.Length == 0)
            {
                attributes = method.ContainingAssembly.GetAttributes(symbolCache.ApiConventionTypeAttribute).ToArray();
            }

            return attributes;
        }

        private void VisitReturnStatementSyntax(
            in ApiConventionContext context,
            ReturnStatementSyntax returnStatementSyntax)
        {
            var returnExpression = returnStatementSyntax.Expression;
            if (returnExpression.IsMissing)
            {
                return;
            }

            var syntaxNodeContext = context.SyntaxNodeContext;

            var typeInfo = syntaxNodeContext.SemanticModel.GetTypeInfo(returnExpression, syntaxNodeContext.CancellationToken);
            if (typeInfo.Type.TypeKind == TypeKind.Error)
            {
                return;
            }

            var type = typeInfo.Type;

            var location = returnStatementSyntax.GetLocation();

            var defaultStatusCodeAttribute = type
                .GetAttributes(context.SymbolCache.DefaultStatusCodeAttribute, inherit: true)
                .FirstOrDefault();

            if (defaultStatusCodeAttribute != null)
            {
                var statusCode = GetDefaultStatusCode(defaultStatusCodeAttribute);
                if (statusCode == null)
                {
                    return;
                }

                context.ActualResponseMetadata.Add(statusCode.Value);
                if (!HasStatusCode(context.ExpectedResponseMetadata, statusCode.Value))
                {
                    context.SyntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode,
                        location,
                        statusCode));
                }
            }
            else if (typeInfo.Type == context.MethodReturnType)
            { 
                if (!HasStatusCode(context.ExpectedResponseMetadata, 200) && !HasStatusCode(context.ExpectedResponseMetadata, 201))
                {
                    context.SyntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MVC1005_ActionReturnsUndocumentedSuccessResult,
                        location));
                }
            }
            else
            {
                // Assume the return type is 200 is we can't infer the return value.  
                context.ActualResponseMetadata.Add(200);
            }
        }

        internal static int? GetDefaultStatusCode(AttributeData attribute)
        {
            if (attribute != null &&
                attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive &&
                attribute.ConstructorArguments[0].Value is int statusCode)
            {
                return statusCode;
            }

            return null;
        }

        internal static ITypeSymbol UnwrapMethodReturnType(
            in ApiControllerSymbolCache symbolCache,
            SemanticModel semanticModel,
            IMethodSymbol method,
            MethodDeclarationSyntax methodSyntax)
        {
            var returnType = method.ReturnType;
            var awaitableReturnType = returnType.InferAwaitableReturnType(semanticModel, methodSyntax.SpanStart);
            returnType = awaitableReturnType ?? returnType;

            if (returnType is INamedTypeSymbol namedReturnType &&
                namedReturnType.IsGenericType &&
                namedReturnType.TypeArguments.Length == 1 &&
                namedReturnType.ConstructedFrom != null &&
                symbolCache.ActionResultOfT.IsAssignableFrom(namedReturnType.ConstructedFrom))
            {
                returnType = namedReturnType.TypeArguments[0];
            }

            return returnType;
        }

        private static bool ShouldEvaluateMethod(ApiControllerSymbolCache symbolCache, IMethodSymbol method, IReadOnlyList<AttributeData> attributes)
        {
            if (attributes.Count == 0)
            {
                return false;
            }

            if (method == null)
            {
                return false;
            }

            if (method.ReturnsVoid || method.ReturnType.TypeKind == TypeKind.Error)
            {
                return false;
            }

            if (!MvcFacts.IsApiController(method.ContainingType, symbolCache.ControllerAttribute, symbolCache.NonControllerAttribute, symbolCache.IApiBehaviorMetadata))
            {
                return false;
            }

            if (!MvcFacts.IsControllerAction(method, symbolCache.NonActionAttribute, symbolCache.IDisposableDispose))
            {
                return false;
            }

            return true;
        }

        internal bool HasStatusCode(IList<ApiResponseMetadata> declaredApiResponseMetadata, int statusCode)
        {
            if (declaredApiResponseMetadata.Count == 0)
            {
                // When no status code is declared, a 200 OK is implied.
                return statusCode == 200;
            }

            for (var i = 0; i < declaredApiResponseMetadata.Count; i++)
            {
                if (declaredApiResponseMetadata[i].StatusCode == statusCode)
                {
                    return true;
                }
            }

            return false;
        }

        internal readonly struct ApiConventionContext
        {
            public ApiConventionContext(
                ApiControllerSymbolCache symbolCache, 
                SyntaxNodeAnalysisContext syntaxNodeContext, 
                IList<ApiResponseMetadata> expectedResponseMetadata, 
                HashSet<int> actualResponseMetadata, 
                ITypeSymbol returnType)
            {
                SymbolCache = symbolCache;
                SyntaxNodeContext = syntaxNodeContext;
                ExpectedResponseMetadata = expectedResponseMetadata;
                ActualResponseMetadata = actualResponseMetadata;
                MethodReturnType = returnType;
            }

            public ApiControllerSymbolCache SymbolCache { get; }
            public SyntaxNodeAnalysisContext SyntaxNodeContext { get; }
            public IList<ApiResponseMetadata> ExpectedResponseMetadata { get; }
            public HashSet<int> ActualResponseMetadata { get; }
            public ITypeSymbol MethodReturnType { get; }
        }
    }
}
