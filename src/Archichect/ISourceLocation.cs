namespace Archichect {
    public interface ISourceLocation {
        string ContainerUri {
            get;
        }
        string AsDipString();
    }
}