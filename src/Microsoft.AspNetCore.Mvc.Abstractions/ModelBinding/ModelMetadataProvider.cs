// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A provider that can supply instances of <see cref="ModelMetadata"/>.
    /// </summary>
    public abstract class ModelMetadataProvider : IModelMetadataProvider
    {
        /// <summary>
        /// Supplies metadata describing the properties of a <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The <see cref="Type"/>.</param>
        /// <returns>A set of <see cref="ModelMetadata"/> instances describing properties of the <see cref="Type"/>.</returns>
        public abstract IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType);

        /// <summary>
        /// Supplies metadata describing a <see cref="Type"/>.
        /// </summary>
        /// <param name="modelType">The <see cref="Type"/>.</param>
        /// <returns>A <see cref="ModelMetadata"/> instance describing the <see cref="Type"/>.</returns>
        public abstract ModelMetadata GetMetadataForType(Type modelType);

        /// <summary>
        /// Supplies metadata describing a parameter.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterInfo"/>.</param>
        /// <returns>A <see cref="ModelMetadata"/> instance describing the <paramref name="parameter"/>.</returns>
        public abstract ModelMetadata GetMetadataForParameter(ParameterInfo parameter);

        /// <summary>
        /// Supplies metadata describing a parameter.
        /// </summary>
        /// <param name="parameter">/// The <see cref="ParameterInfo"/></param>
        /// <param name="modelType">The model type.</param>
        /// <returns>A <see cref="ModelMetadata"/> instance describing the <paramref name="parameter"/>.</returns>
        /// <remarks>
        /// <para>
        /// This methods supports model binders that return polymorphic results i.e. the actual model bound type is an interface
        /// implementation or subtype of the original type.
        /// </para>
        /// <para>
        /// <paramref name="modelType"/> is the actual model bound type which may not match the parameter type specified by
        /// <paramref name="parameter"/>. It is expected that the caller avoids calling this method if the two types are the same.
        /// </para>
        /// </remarks>
        public virtual ModelMetadata GetMetadataForParameter(ParameterInfo parameter, Type modelType)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Supplies metadata describing a property.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/>.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>A <see cref="ModelMetadata"/> instance describing the <paramref name="propertyInfo"/>.</returns>
        /// <remarks>
        /// <para>
        /// This methods supports model binders that return polymorphic results i.e. the actual model bound type is an interface
        /// implementation or subtype of the original type.
        /// </para>
        /// <para>
        /// <paramref name="modelType"/> is the actual model bound type which may not match the parameter type specified by
        /// <paramref name="propertyInfo"/>. It is expected that the caller avoids calling this method if the two types are the same.
        /// </para>
        /// </remarks>
        public virtual ModelMetadata GetMetadataForProperty(PropertyInfo propertyInfo, Type modelType)
        {
            throw new NotSupportedException();
        }
    }
}
