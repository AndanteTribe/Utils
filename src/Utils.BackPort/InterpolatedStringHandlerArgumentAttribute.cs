﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET6_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    /// <summary>Indicates which arguments to a method involving an interpolated string handler should be passed to that handler.</summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="InterpolatedStringHandlerArgumentAttribute"/> class.</summary>
        /// <param name="argument">The name of the argument that should be passed to the handler.</param>
        /// <remarks>The empty string may be used as the name of the receiver in an instance method.</remarks>
        public InterpolatedStringHandlerArgumentAttribute(string argument) => Arguments = new string[] { argument };

        /// <summary>Initializes a new instance of the <see cref="InterpolatedStringHandlerArgumentAttribute"/> class.</summary>
        /// <param name="arguments">The names of the arguments that should be passed to the handler.</param>
        /// <remarks>The empty string may be used as the name of the receiver in an instance method.</remarks>
        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) => Arguments = arguments;

        /// <summary>Gets the names of the arguments that should be passed to the handler.</summary>
        /// <remarks>The empty string may be used as the name of the receiver in an instance method.</remarks>
        public string[] Arguments { get; }
    }
}

#endif