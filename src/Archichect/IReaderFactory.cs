using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Archichect {
    public abstract class AbstractReadingContext {
        private IEnumerable<IDependencyReader> _readerGang;

        private int _readCount;
        private int _stepReadCount;
        private readonly Stopwatch _readTime = new Stopwatch();
        private readonly Stopwatch _stepReadTime = new Stopwatch();

        public IEnumerable<IDependencyReader> ReaderGang => _readerGang;

        public void SetReaderGang(IEnumerable<IDependencyReader> readerGang) {
            if (readerGang == null) {
                throw new ArgumentNullException(nameof(readerGang));
            }
            if (_readerGang != null) {
                throw new InvalidOperationException("SetReaderGang can only be called once");
            }
            _readerGang = readerGang;
            _readTime.Start();
        }

        public int ReadCount => _readCount;
        public int StepReadCount => _stepReadCount;

        public void StartStep() {
            _stepReadCount = 0;
            _stepReadTime.Restart();
        }

        public void FinishStep() {
            _stepReadTime.Stop();
        }

        public TimeSpan FullTime => _readTime.Elapsed;

        public TimeSpan StepTime => _stepReadTime.Elapsed;

        public void IncCount(int inc) {
            _readCount += inc;
            _stepReadCount += inc;
        }

        public abstract void Finish();
    }

    public interface IReaderFactory : IPlugin {
        [NotNull, ItemNotNull]
        IEnumerable<string> SupportedFileExtensions { get; }
        [NotNull]
        IDependencyReader CreateReader([NotNull] string fileName, bool needsOnlyItemTails, AbstractReadingContext readingContext);
        [NotNull]
        AbstractReadingContext CreateReadingContext();
    }

    public interface IDependencyReader {
        string FullFileName { get; }

        IEnumerable<Dependency> ReadDependencies(WorkingGraph readingGraph, int depth, bool ignoreCase);
    }
}