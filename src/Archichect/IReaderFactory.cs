using System.Collections.Generic;
using JetBrains.Annotations;

namespace Archichect {
    public interface IReadingContext {
        void AfterReading();
    }

    public interface IReaderFactory : IPlugin {
        [NotNull, ItemNotNull]
        IEnumerable<string> SupportedFileExtensions { get; }
        [NotNull]
        IDependencyReader CreateReader([NotNull] string fileName, bool needsOnlyItemTails, IReadingContext readingContext);
        [CanBeNull]
        IReadingContext CreateReadingContext();
    }

    public interface IDependencyReader {
        string FullFileName { get; }

        void SetReadersInSameReadFilesBeforeReadDependencies(IDependencyReader[] readerGang);
        IEnumerable<Dependency> ReadDependencies(WorkingGraph readingGraph, int depth, bool ignoreCase);
    }
}