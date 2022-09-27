#pragma warning disable
#define INTERNAL_NULLABLE_ATTRIBUTES

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
#if NETSTANDARD2_1 ||  NETSTANDARD2_0 ||  NETCOREAPP2_0 ||  NETCOREAPP2_1 ||  NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string? parameterName)
        {
            ParameterName = parameterName;
        }

        public string? ParameterName { get; }
    }
}

#endif