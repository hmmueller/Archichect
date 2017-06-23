using System.Collections.Generic;
using JetBrains.Annotations;

namespace Archichect.Reading {
    public abstract class AbstractReaderFactory : IReaderFactory {
        [NotNull, ItemNotNull]
        public abstract IEnumerable<string> SupportedFileExtensions { get; }

        [NotNull]
        public abstract IDependencyReader CreateReader([NotNull] string fileName, bool needsOnlyItemTails, IReadingContext readingContext);

        [NotNull]
        public abstract string GetHelp(bool detailedHelp, string filter);

        public abstract IReadingContext CreateReadingContext();
    }
}