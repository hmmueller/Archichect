using System.Collections.Generic;

namespace Archichect.Reading.DipReading {
    public class DipReaderFactory : AbstractReaderFactory {
        public override IDependencyReader CreateReader(string fileName, bool needsOnlyItemTails, IReadingContext readingContext) {
            return new DipReader(fileName);
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

        public override IReadingContext CreateReadingContext() {
            return null;
        }
    }
}