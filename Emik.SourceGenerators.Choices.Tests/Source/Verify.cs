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
