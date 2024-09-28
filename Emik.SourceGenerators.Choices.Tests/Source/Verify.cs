// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

public sealed class Verify : CSharpSourceGeneratorTest<ExtendingGenerator, DefaultVerifier>
{
    /// <summary>Gets the root of steam, if installed.</summary>
    static string? SteamRoot { get; } =
        Environment.GetEnvironmentVariable("STEAM_ROOT") ??
        (OperatingSystem.IsWindows() &&
            $@"HKEY_LOCAL_MACHINE\SOFTWARE\{(Environment.Is64BitOperatingSystem ? @"WOW6432Node\" : "")}Valve\Steam"
                is var keyName ? Registry.GetValue(keyName, "InstallPath", null) as string :
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) is var home &&
            OperatingSystem.IsMacOS() ? Path.Join(home, "Library/Application Support/Steam") :
            OperatingSystem.IsLinux() ? Path.Join(home, ".steam/steam") : null);

    /// <summary>Gets the directory for additional assemblies.</summary>
    static string AssemblyDirectory { get; } =
        Path.Join(SteamRoot, "steamapps", "common", "Keep Talking and Nobody Explodes", "ktane_Data", "Managed");

    /// <inheritdoc />
    protected override IEnumerable<Type> GetSourceGenerators() =>
        base.GetSourceGenerators().Prepend(typeof(AttributeGenerator));

    /// <inheritdoc />
    protected override ParseOptions CreateParseOptions() =>
        ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion.Preview);

    /// <inheritdoc />
    protected override async Task<Project> CreateProjectImplAsync(
        EvaluatedProjectState primaryProject,
        ImmutableArray<EvaluatedProjectState> additionalProjects,
        CancellationToken token
    ) =>
        (await base.CreateProjectImplAsync(primaryProject, additionalProjects, token)).AddMetadataReferences(
            ((string[])["KMFramework.dll", "UnityEngine.dll"])
           .Select(x => Path.Join(AssemblyDirectory, x))
           .Where(File.Exists)
           .Select(x => MetadataReference.CreateFromFile(x))
        );
}

class A
{
    readonly byte _discriminator;

    /// <summary>
    /// Determines whether the left-hand side is greater than the right.
    /// </summary>
    /// <param name="left">The left-hand side.</param>
    /// <param name="right">The right-hand side.</param>
    /// <returns>
    /// The value determining whether the parameter <paramref name="left"/>
    /// is greater than the parameter <paramref name="right"/>.
    /// </returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator >(A left, A right)
        => left._discriminator > right._discriminator ||
            left._discriminator == right._discriminator &&
            left._discriminator switch
            {
                0 => false,
                _ => false,
            };
}
