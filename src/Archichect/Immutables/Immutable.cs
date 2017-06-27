using JetBrains.Annotations;

namespace Archichect.Immutables {
    public interface IImmutable { }

    public abstract class Immutable<TImmutable, TMutable> : IImmutable
        where TImmutable : Immutable<TImmutable, TMutable>, IImmutable
        where TMutable : Mutable<TImmutable, TMutable>, IMutable {
        protected abstract TMutable CreateMutable();

        public TMutable GetOrCreateMutable(ImmutableSupport support) {
            return support.GetOrCreateMutable(this, () => CreateMutable());
        }

        protected internal abstract TImmutable Immutify([CanBeNull] TMutable mutable, ImmutableSupport support);
    }
}
