// SPDX-License-Identifier: MPL-2.0
using Emik.SourceGenerators.Choices.Generated.Tests;

Fruit fruit = 3;

[Choice.Apple<byte>.Pear<int>.Orange<BindingFlags>]
sealed partial class Fruit;
