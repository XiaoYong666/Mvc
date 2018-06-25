// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
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
            DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var symbolCache = new ApiControllerSymbolCache(compilationStartAnalysisContext.Compilation);
                if (symbolCache.ApiConventionAttribute == null || symbolCache.ApiConventionAttribute.TypeKind == TypeKind.Error)
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
                var symbolInfo = syntaxNodeContext.SemanticModel.GetSymbolInfo(methodSyntax, syntaxNodeContext.CancellationToken);
                if (symbolInfo.Symbol == null)
                {
                    return;
                }

                var method = (IMethodSymbol)symbolInfo.Symbol;

                if (!MvcFacts.IsApiController(method.ContainingType, symbolCache.ControllerAttribute, symbolCache.NonControllerAttribute, symbolCache.ApiConventionAttribute))
                {
                    return;
                }

                if (!MvcFacts.IsControllerAction(method, symbolCache.NonActionAttribute, symbolCache.IDisposableDispose))
                {
                    return;
                }

                var expectedResponseMetadata = SymbolApiResponseMetadataProvider.GetResponseMetadata(symbolCache, method);

                

            }, SyntaxKind.MethodDeclaration);
        }

        private class ReturnStatementVisitor : SymbolVisitor<IMethodSymbol>
        {

        }
    }
}
