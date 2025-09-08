using NUnit.Framework;

// Configure NUnit parallelization for ReactiveUI's static-heavy codebase
// Run test fixtures in parallel, but tests within each fixture sequentially
// This prevents concurrency issues with ReactiveUI's static classes
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]