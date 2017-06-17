using JetBrains.Annotations;

namespace Archichect {
    /// <summary>
    /// Base interface for all plugins - IReaderFactory, ITransformer, ICalculator and IRenderer.
    /// </summary>
    public interface IPlugin {
        [NotNull]
        string GetHelp(bool detailedHelp, string filter);
    }
}