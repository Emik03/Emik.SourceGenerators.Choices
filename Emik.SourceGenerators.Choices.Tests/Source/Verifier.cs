// SPDX-License-Identifier: MPL-2.0
using Task = System.Threading.Tasks.Task;

namespace Emik.SourceGenerators.Choices.Tests;

#pragma warning disable 169
public sealed class Verifier : CSharpSourceGeneratorTest<ExtendingGenerator, DefaultVerifier>
{
    public static Task GoAsync() => new Verifier().RunAsync();

    /// <summary>Gets the root of steam, if installed.</summary>
    static string? SteamRoot { get; } =
        Environment.GetEnvironmentVariable("STEAM_ROOT") ??
        (OperatingSystem.IsWindows() &&
            $@"HKEY_LOCAL_MACHINE\SOFTWARE\{(Environment.Is64BitOperatingSystem ? @"WOW6432Node\" : "")}Valve\Steam"
                is var keyName ? Registry.GetValue(keyName, "InstallPath", null) as string :
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) is var home &&
            OperatingSystem.IsMacOS() ? Path.Join(home, "Library/Application Support/Steam") :
            OperatingSystem.IsLinux() ? Path.Join(home, ".steam/steam") : null);

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
        CancellationToken cancellationToken
    )
    {
        var directory = Path.Join(
            SteamRoot,
            "steamapps",
            "common",
            "Keep Talking and Nobody Explodes",
            "ktane_Data",
            "Managed"
        );

        return (await base.CreateProjectImplAsync(primaryProject, additionalProjects, cancellationToken))
           .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(directory, "KMFramework.dll")))
           .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(directory, "UnityEngine.dll")));
    }
}
