// SPDX-License-Identifier: MPL-2.0
using static Microsoft.CodeAnalysis.CSharp.LanguageVersion;

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
            OperatingSystem.IsMacOS() ? Path.Join(home, "Library", "Application Support", "Steam") :
            Environment.GetEnvironmentVariable("XDG_DATA_DIR") is { } xdg &&
            Path.Join(xdg, "Steam") is var xdgSteam &&
            Path.Exists(xdgSteam) ? xdgSteam : Path.Join(home, ".local", "share", "Steam"));

    /// <summary>Gets the root of unity, if installed.</summary>
    static string? UnityRoot { get; } =
        ((Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) is var home &&
            Path.Join(home, "Unity", "Hub", "Editor") is var linux &&
            Directory.Exists(linux) ? linux :
            Path.Join(home, "Applications", "Unity", "Hub", "Editor") is var macos &&
            Directory.Exists(macos) ? macos :
            Path.Join("C:", "Program Files", "Unity", "Hub", "Editor") is var windows &&
            Directory.Exists(windows) ? windows : null) is { } unityHub
            ? Directory.GetDirectories(unityHub)
            : Directory.GetDirectories(home, "Unity-*")).FirstOrDefault();

    static ImmutableArray<PortableExecutableReference> KMFramework { get; } =
        Path.Join(
            SteamRoot,
            "steamapps",
            "common",
            "Keep Talking and Nobody Explodes",
            "ktane_Data",
            "Managed",
            "KMFramework.dll"
        ) is var km &&
        File.Exists(km)
            ? [MetadataReference.CreateFromFile(km)]
            : [];

    static ImmutableArray<PortableExecutableReference> UnityEngine { get; } =
        Path.Join(UnityRoot, "Editor", "Data", "Managed", "UnityEngine.dll") is var unity && File.Exists(unity)
            ? [MetadataReference.CreateFromFile(unity)]
            : [];

    /// <inheritdoc />
    protected override IEnumerable<Type> GetSourceGenerators() =>
        base.GetSourceGenerators().Prepend(typeof(AttributeGenerator));

    /// <inheritdoc />
    protected override ParseOptions CreateParseOptions() =>
        ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(Preview);

    /// <inheritdoc />
    protected override async Task<Project> CreateProjectImplAsync(
        EvaluatedProjectState primaryProject,
        ImmutableArray<EvaluatedProjectState> additionalProjects,
        CancellationToken token
    ) =>
        (await base.CreateProjectImplAsync(primaryProject, additionalProjects, token))
       .WithMetadataReferences(Net100.References.All)
       .AddMetadataReferences(KMFramework)
       .AddMetadataReferences(UnityEngine);
}
