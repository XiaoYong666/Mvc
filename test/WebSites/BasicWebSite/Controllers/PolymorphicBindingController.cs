// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BasicWebSite.Models;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebSite.Controllers
{
    public class PolymorphicBindingController : ControllerBase
    {
        public IActionResult ModelBound([ModelBinder(typeof(PolymorphicBinder))] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(person);
        }

        [HttpPost]
        public IActionResult InputFormatted([FromBody] IPerson person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(person);
        }
    }
}
