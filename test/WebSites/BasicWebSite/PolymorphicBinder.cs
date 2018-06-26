// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BasicWebSite.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BasicWebSite.Controllers
{
    public class PolymorphicBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var model = new DerivedPerson
            {
                DerivedProperty = bindingContext.ValueProvider.GetValue(nameof(DerivedPerson.DerivedProperty)).FirstValue,
            };

            bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }
    }
}