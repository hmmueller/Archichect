using System;
using System.Collections.Generic;
using System.Linq;

namespace Archichect.Immutables {
    public class ImmutableSupport {
        private readonly Dictionary<IImmutable, IMutable> _immutableToMutable = new Dictionary<IImmutable, IMutable>();
        private readonly Dictionary<IImmutable, IImmutable> _immutableToImmutable = new Dictionary<IImmutable, IImmutable>();

        public TMutable GetOrCreateMutable<TMutable>(IImmutable immutable, Func<TMutable> createMutable)
            where TMutable : IMutable {
            IMutable result;
            if (!_immutableToMutable.TryGetValue(immutable, out result)) {
                _immutableToMutable.Add(immutable, result = createMutable());
            }
            return (TMutable)result;
        }

        public TImmutable Immutify<TImmutable, TMutable>(TImmutable immutable)
            where TImmutable : Immutable<TImmutable, TMutable>, IImmutable
            where TMutable : Mutable<TImmutable, TMutable>, IMutable {
            if (immutable == null) {
                return null;
            } else {
                IMutable mutable;
                _immutableToMutable.TryGetValue(immutable, out mutable);
                IImmutable result;
                if (mutable == null || !_immutableToImmutable.TryGetValue(immutable, out result)) {
                    _immutableToImmutable.Add(immutable, result = immutable.Immutify((TMutable) mutable, this));
                }
                return (TImmutable) result;
            }
        }

        public IEnumerable<TImmutable> Immutify<TImmutable, TMutable>(IEnumerable<TImmutable> immutables)
            where TImmutable : Immutable<TImmutable, TMutable>, IImmutable
            where TMutable : Mutable<TImmutable, TMutable>, IMutable {
            TImmutable[] result = immutables.Select(x => Immutify<TImmutable, TMutable>(x)).ToArray();
            bool allEqual = immutables.Select((immutable, i) => Equals(immutable, result[i])).All(b => b);
            return allEqual ? immutables : result;
        }
    }
}