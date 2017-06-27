namespace Archichect.Immutables {
    public interface IMutable { }

    public abstract class Mutable<TImmutable, TMutable> : IMutable
        where TImmutable : Immutable<TImmutable, TMutable>, IImmutable
        where TMutable : Mutable<TImmutable, TMutable>, IMutable {
    }
}