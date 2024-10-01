// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.


namespace Darp.Utils.ResxSourceGenerator.Tests.Verifiers;

using Microsoft.CodeAnalysis;

public static partial class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new() { }
