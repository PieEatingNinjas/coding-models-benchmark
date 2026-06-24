using System.Runtime.CompilerServices;
using Bogus;
using Xunit;

// Deterministic test data: fix Bogus' global seed and disable test parallelization
// so the seed is consumed in a stable order. This keeps the benchmark's fake data
// identical across runs and across models — the model is the only variable.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TodoApi.Tests.Support;

internal static class TestSeed
{
    [ModuleInitializer]
    internal static void Init() => Randomizer.Seed = new Random(8675309);
}
