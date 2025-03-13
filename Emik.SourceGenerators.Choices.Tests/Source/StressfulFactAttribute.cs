// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

/// <summary>Indicates a unit test that should only be run when <see cref="EnvironmentVariable"/> is defined.</summary>
public sealed class StressfulFactAttribute : FactAttribute
{
    /// <summary>The environment variable to check.</summary>
    const string EnvironmentVariable = "IS_STRESS_MODE_ENABLED";

    /// <summary>Initializes a new instance of the <see cref="StressfulFactAttribute"/> class.</summary>
    public StressfulFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(EnvironmentVariable) is null or "")
            Skip = "Stress test was skipped because the environment variable IS_STRESS_MODE_ENABLED was not defined.";
    }
}
