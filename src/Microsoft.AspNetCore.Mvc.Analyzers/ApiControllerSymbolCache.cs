// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

<<<<<<< HEAD
=======
using System;
>>>>>>> WIP
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    internal readonly struct ApiControllerSymbolCache
    {
        public ApiControllerSymbolCache(Compilation compilation)
        {
<<<<<<< HEAD
            ApiConventionTypeAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionTypeAttribute);
            ApiConventionNameMatchAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionNameMatchAttribute);
            ApiConventionTypeMatchAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionTypeMatchAttribute);
            ProducesResponseTypeAttribute = compilation.GetTypeByMetadataName(SymbolNames.ProducesResponseTypeAttribute);
        }

        public INamedTypeSymbol ApiConventionNameMatchAttribute { get; }

        public INamedTypeSymbol ApiConventionTypeMatchAttribute { get; }

        public INamedTypeSymbol ApiConventionTypeAttribute { get; }
=======
            ApiConventionAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionAttribute);
            ControllerAttribute = compilation.GetTypeByMetadataName(SymbolNames.ControllerAttribute);
            IApiBehaviorMetadata = compilation.GetTypeByMetadataName(SymbolNames.IApiBehaviorMetadata);
            NonActionAttribute = compilation.GetTypeByMetadataName(SymbolNames.NonActionAttribute);
            NonControllerAttribute = compilation.GetTypeByMetadataName(SymbolNames.NonControllerAttribute);
            ProducesResponseTypeAttribute = compilation.GetTypeByMetadataName(SymbolNames.ProducesResponseTypeAttribute);

            var disposable = compilation.GetSpecialType(SpecialType.System_IDisposable);
            var members = disposable.GetMembers(nameof(IDisposable.Dispose));
            IDisposableDispose = members.Length == 1 ? (IMethodSymbol)members[0] : null;
        }

        public INamedTypeSymbol ApiConventionAttribute { get; }

        public INamedTypeSymbol ControllerAttribute { get; }

        public INamedTypeSymbol IApiBehaviorMetadata { get; }

        public IMethodSymbol IDisposableDispose { get; }

        public INamedTypeSymbol NonActionAttribute { get; }

        public INamedTypeSymbol NonControllerAttribute { get; }
>>>>>>> WIP

        public INamedTypeSymbol ProducesResponseTypeAttribute { get; }
    }
}
