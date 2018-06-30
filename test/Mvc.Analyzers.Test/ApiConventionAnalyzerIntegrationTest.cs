﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class ApiConventionAnalyzerIntegrationTest
    {
        private MvcDiagnosticAnalyzerRunner Executor { get; } = new MvcDiagnosticAnalyzerRunner(new ApiConventionAnalyzer());

        [Fact]
        public Task NoDiagnosticsAreReturned_ForNonApiController()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task NoDiagnosticsAreReturned_ForRazorPageModels()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task NoDiagnosticsAreReturned_ForApiController_WithAllDocumentedStatusCodes()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task NoDiagnosticsAreReturned_ForApiController_WithoutApiConventions()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task DiagnosticsAreReturned_IfMethodWithProducesResponseTypeAttribute_ReturnsUndocumentedStatusCode()
            => RunTest(DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode);

        [Fact]
        public Task DiagnosticsAreReturned_IfAsyncMethodWithProducesResponseTypeAttribute_ReturnsUndocumentedStatusCode()
            => RunTest(DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode);

        [Fact]
        public Task DiagnosticsAreReturned_IfAsyncMethodReturningValueTaskWithProducesResponseTypeAttribute_ReturnsUndocumentedStatusCode()
            => RunTest(DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode);

        [Fact]
        public Task DiagnosticsAreReturned_ForActionResultOfTReturningMethodWithoutAnyAttributes()
            => RunTest(DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode);

        [Fact]
        public Task DiagnosticsAreReturned_ForActionResultOfTReturningMethodWithoutSomeAttributes()
            => RunTest(DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode);

        private async Task RunNoDiagnosticsAreReturned([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var testSource = MvcTestSource.Read(GetType().Name, testMethod);
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Executor.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Empty(result);
        }

        private async Task RunTest(DiagnosticDescriptor descriptor, [CallerMemberName] string testMethod = "")
        {
            // Arrange
            var testSource = MvcTestSource.Read(GetType().Name, testMethod);
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Executor.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Collection(
                result,
                diagnostic =>
                {
                    Assert.Equal(descriptor.Id, diagnostic.Id);
                    Assert.Same(descriptor, diagnostic.Descriptor);
                    AnalyzerAssert.DiagnosticLocation(expectedLocation, diagnostic.Location);
                });
        }
    }
}