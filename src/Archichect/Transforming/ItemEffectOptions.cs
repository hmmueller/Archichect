using System.Collections.Generic;

namespace Archichect.Transforming {
    public class ItemEffectOptions : EffectOptions<Item> {
        public ItemEffectOptions() : base("item") {
        }

        public IEnumerable<Option> AllOptions => BaseOptions;
    }
}