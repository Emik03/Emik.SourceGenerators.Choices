# Emik.SourceGenerators.Choices

[![NuGet package](https://img.shields.io/nuget/v/Emik.SourceGenerators.Choices.svg?color=50fa7b&logo=NuGet&style=for-the-badge)](https://www.nuget.org/packages/Emik.SourceGenerators.Choices)
[![License](https://img.shields.io/github/license/Emik03/Emik.SourceGenerators.Choices.svg?color=6272a4&style=for-the-badge)](https://github.com/Emik03/Emik.SourceGenerators.Choices/blob/main/LICENSE)

Annotate `[Choice]` to transform your type into a highly performant and flexible disjoint union.

Massive thanks to [hikarin522's ValueVariant](https://github.com/hikarin522/ValueVariant) for being the inspiration to this source generator.

This source generator assumes the project it is generating for to be at least C# version 6.0, or 8.0 if the variants include pointer types or ref-like types.

This project has a dependency to [Emik.Morsels](https://github.com/Emik03/Emik.Morsels), if you are building this project, refer to its [README](https://github.com/Emik03/Emik.Morsels/blob/main/README.md) first.

---

- [Examples](#examples)
- [Contribute](#contribute)
- [License](#license)

---

## Examples

#### Usages

```csharp
Result<string, int> result = "Success"; // Implicit conversion to `Ok`.
Encoding encoding = default; // Defaults to first 
Result result = new(ok: 1m); // Constructor for `Ok`.
Option<string> option = Option.OfSome("Hello"); // Factory method to `Some`.

if (result.IsOk) // Checks for whether the union is `Ok`.
    Console.WriteLine(result.Ok.GetHashCode()); // `Ok` is flagged as non-null.

var length = encoding.Length; // Since both variants have a `Length` property, you can access it from the union directly.
encoding.Clear(); // Since both variants have a `Clear` method, you can invoke it from the union directly.
var value = result.GetUnderlyingValue(); // Returns `ISerializable` since both variants implement or are `ISerializable`.
var isEmpty = option.Map(x => x.Length is 0, () => true); // Exhaustive match.
var maybe = (string?)option; // Explicit cast back to `string?`.
```

#### Declarations

```csharp
using Emik;

[Choice]
readonly partial record struct Result<TOk, TErr>(TOk? ok, TErr? err);

[Choice.Utf8<Span<byte>>.Utf16<Span<char>>]
ref partial struct Encoding;

[Choice(typeof((ISerializable Ok, Exception Err)))]
sealed partial class Result;

[Choice]
readonly partial struct Option<T>
{
    readonly T _some;

    ValueTuple None => default;
}
```

For more examples, take a look at the [playground](https://github.com/Emik03/Emik.SourceGenerators.Choices/blob/main/Emik.SourceGenerators.Choices.Generated.Tests/Source/Playground.cs) or the [unit tests](https://github.com/Emik03/Emik.SourceGenerators.Choices/blob/main/Emik.SourceGenerators.Choices.Tests/Source/Case.cs), and the [expected output](https://github.com/Emik03/Emik.SourceGenerators.Choices/tree/main/Emik.SourceGenerators.Choices.Tests/Expected).

## Contribute

Issues and pull requests are welcome to help this repository be the best it can be.

## License

This repository falls under the [MPL-2 license](https://www.mozilla.org/en-US/MPL/2.0/).
