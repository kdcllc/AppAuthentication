﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace AppAuthentication.Helpers
{
#pragma warning disable RCS1102 // Make class static.
    internal class ValidationHelper
#pragma warning restore RCS1102 // Make class static.
    {
        /// <summary>
        /// Validates a resource identifier.
        /// </summary>
        /// <param name="resource">The resource to validate.</param>
        public static void ValidateResource(string resource)
        {
            if (!Regex.IsMatch(resource, @"^[0-9a-zA-Z-.:/]+$"))
            {
                throw new ArgumentException($"Resource {resource} is not in expected format. Only alphanumeric characters, [dot], [colon], [hyphen], and [forward slash] are allowed.");
            }
        }
    }
}