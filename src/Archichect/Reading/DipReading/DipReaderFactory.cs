using System.Collections.Generic;
using JetBrains.Annotations;

namespace Archichect.Reading.DipReading {
    public class DipReaderFactory : AbstractReaderFactory {
        public class ReadingContext : AbstractReadingContext {
            public override void Finish() {
                // empty
            }
        }

        public override IDependencyReader CreateReader([NotNull] string fileName, bool needsOnlyItemTails, 
            [NotNull] AbstractReadingContext readingContext) {
            return new DipReader(fileName, (ReadingContext) readingContext);
        }

        private static readonly string[] _supportedFileExtensions = { ".dip" };

        public override IEnumerable<string> SupportedFileExtensions => _supportedFileExtensions;

        public override string GetHelp(bool detailedHelp, string filter) {
            string result = @"Read data from .dip file. 

The itemtypes of the read dependencies are defined in the .dip file.";
            if (detailedHelp) {
                result += @"

.dip file format:

___EXPLANATION MISSING___";

            }
            return result;
        }

        public override AbstractReadingContext CreateReadingContext() {
            return new ReadingContext();
        }
    }
}