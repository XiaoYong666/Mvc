// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    internal static class CodeAnalysisExtensions
    {
        public static bool HasAttribute(this ITypeSymbol typeSymbol, ITypeSymbol attribute, bool inherit)
            => GetAttributes(typeSymbol, attribute, inherit).Any();

        public static bool HasAttribute(this IMethodSymbol methodSymbol, ITypeSymbol attribute, bool inherit)
            => GetAttributes(methodSymbol, attribute, inherit).Any();

        public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, ITypeSymbol attribute)
        {
            foreach (var declaredAttribute in symbol.GetAttributes())
            {
                if (attribute.IsAssignableFrom(declaredAttribute.AttributeClass))
                {
                    yield return declaredAttribute;
                }
            }
        }

        public static IEnumerable<AttributeData> GetAttributes(this IMethodSymbol methodSymbol, ITypeSymbol attribute, bool inherit)
        {
            Debug.Assert(methodSymbol != null);
            Debug.Assert(attribute != null);

            while (methodSymbol != null)
            {
                foreach (var attributeData in GetAttributes(methodSymbol, attribute))
                {
                    yield return attributeData;
                }

                if (!inherit)
                {
                    break;
                }

                methodSymbol = methodSymbol.IsOverride ? methodSymbol.OverriddenMethod : null;
            }
        }

        public static IEnumerable<AttributeData> GetAttributes(this ITypeSymbol typeSymbol, ITypeSymbol attribute, bool inherit)
        {
            Debug.Assert(typeSymbol != null);
            Debug.Assert(attribute != null);

            foreach (var type in GetTypeHierarchy(typeSymbol))
            {
                foreach (var attributeData in GetAttributes(type, attribute))
                {
                    yield return attributeData;
                }

                if (!inherit)
                {
                    break;
                }
            }
        }

        public static bool HasAttribute(this IPropertySymbol propertySymbol, ITypeSymbol attribute, bool inherit)
        {
            Debug.Assert(propertySymbol != null);
            Debug.Assert(attribute != null);

            if (!inherit)
            {
                return HasAttribute(propertySymbol, attribute);
            }

            while (propertySymbol != null)
            {
                if (propertySymbol.HasAttribute(attribute))
                {
                    return true;
                }

                propertySymbol = propertySymbol.IsOverride ? propertySymbol.OverriddenProperty : null;
            }

            return false;
        }

        public static bool IsAssignableFrom(this ITypeSymbol source, ITypeSymbol target)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);

            if (source.TypeKind == TypeKind.Interface)
            {
                foreach (var @interface in target.AllInterfaces)
                {
                    if (source == @interface)
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var type in target.GetTypeHierarchy())
            {
                if (source == type)
                {
                    return true;
                }
            }

            return false;
        }

        // Based on http://source.roslyn.io/#Microsoft.CodeAnalysis.Features/Shared/Extensions/ISymbolExtensions_2.cs,299
        public static ITypeSymbol InferAwaitableReturnType(this ITypeSymbol typeSymbol, SemanticModel semanticModel, int position)
        {
            var potentialGetAwaiters = semanticModel.LookupSymbols(
                position, 
                container: typeSymbol, 
                name: WellKnownMemberNames.GetAwaiter, 
                includeReducedExtensionMethods: true);
            var getAwaiters = potentialGetAwaiters.OfType<IMethodSymbol>().Where(x => !x.Parameters.Any());
            if (!getAwaiters.Any())
            {
                return null;
            }

            var getResults = getAwaiters.SelectMany(g => semanticModel.LookupSymbols(
                position, 
                container: g.ReturnType, 
                name: WellKnownMemberNames.GetResult));

            var getResult = getResults.OfType<IMethodSymbol>().FirstOrDefault(g => !g.IsStatic);
            if (getResult == null)
            {
                return null;
            }

            return getResult.ReturnType;
        }

        private static bool HasAttribute(this ISymbol symbol, ITypeSymbol attribute)
        {
            foreach (var declaredAttribute in symbol.GetAttributes())
            {
                if (attribute.IsAssignableFrom(declaredAttribute.AttributeClass))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<ITypeSymbol> GetTypeHierarchy(this ITypeSymbol typeSymbol)
        {
            while (typeSymbol != null)
            {
                yield return typeSymbol;

                typeSymbol = typeSymbol.BaseType;
            }
        }

        private static bool VerifyGetAwaiter(IMethodSymbol getAwaiter)
        {
            var returnType = getAwaiter.ReturnType;
            if (returnType == null)
            {
                return false;
            }

            // bool IsCompleted { get }
            if (!returnType.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == WellKnownMemberNames.IsCompleted && p.Type.SpecialType == SpecialType.System_Boolean && p.GetMethod != null))
            {
                return false;
            }

            var methods = returnType.GetMembers().OfType<IMethodSymbol>();

            // void OnCompleted(Action) 
            // Actions are delegates, so we'll just check for delegates.
            if (!methods.Any(x => x.Name == WellKnownMemberNames.OnCompleted && x.ReturnsVoid && x.Parameters.Length == 1 && x.Parameters.First().Type.TypeKind == TypeKind.Delegate))
            {
                return false;
            }

            // void GetResult() || T GetResult()
            return methods.Any(m => m.Name == WellKnownMemberNames.GetResult && !m.Parameters.Any());
        }
    }
}
