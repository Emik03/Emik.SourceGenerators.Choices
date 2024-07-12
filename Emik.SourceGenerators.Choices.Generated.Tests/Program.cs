// SPDX-License-Identifier: MPL-2.0
using Emik.SourceGenerators.Choices.Generated.Tests;

var a = DotKMModule.OfOther(new());
var b = a.GetUnderlyingValue();

Console.WriteLine(b);
