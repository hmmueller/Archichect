using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Archichect.Matching;

namespace Archichect.Transforming {
    public class FromTo<TItem, TDependency>
            where TDependency : AbstractDependency<TItem>
            where TItem : AbstractItem<TItem> {
        [NotNull]
        public readonly TItem From;
        [NotNull]
        public readonly TItem To;

        private readonly int _hash;
        private readonly int _eqHash;

        public FromTo([NotNull] TItem from, [NotNull] TItem to) {
            From = from;
            To = to;

            _hash = unchecked(17 * From.GetHashCode() + 650069881 * To.GetHashCode());
            _eqHash = unchecked(38795333 * From.GetEqHashCode() + 23 * To.GetEqHashCode());
        }

        public override bool Equals(object obj) {
            var other = obj as FromTo;
            return other != null
                && other._eqHash == _eqHash
                && other._hash == _hash
                && other.From.Equals(From) 
                && other.To.Equals(To);
        }

        public override int GetHashCode() {
            return _hash;
        }
    }

    public class FromTo : FromTo<Item, Dependency> {
        public FromTo([NotNull] Item from, [NotNull] Item to) : base(from, to) {
        }

        public FromTo AggregateDependency(WorkingGraph graph, Dependency d, Dictionary<FromTo, Dependency> edgeCollector) {
            Dependency result;
            if (!edgeCollector.TryGetValue(this, out result)) {
                result = graph.CreateDependency(From, To, d.Source, d.MarkerSet, d.Ct, d.QuestionableCt, d.BadCt,
                d.NotOkReason, d.ExampleInfo);
                edgeCollector.Add(this, result);
            } else {
                result.AggregateMarkersAndCounts(d);
            }
            result.UsingItem.MergeWithMarkers(d.UsingItem.MarkerSet);
            result.UsedItem.MergeWithMarkers(d.UsedItem.MarkerSet);
            return this;
        }

        public static Dictionary<FromTo, Dependency> AggregateAllDependencies(WorkingGraph graph,
            [NotNull, ItemNotNull] IEnumerable<Dependency> dependencies) {
            var result = new Dictionary<FromTo, Dependency>(dependencies.Count());
            foreach (var d in dependencies) {
                new FromTo(d.UsingItem, d.UsedItem).AggregateDependency(graph, d, result);
            }
            return result;
        }

        public static bool ContainsMatchingDependency(Dictionary<FromTo, Dependency> fromTos, Item from, Item to,
            DependencyPattern patternOrNull = null) {
            Dependency fromTo;
            return fromTos.TryGetValue(new FromTo(from, to), out fromTo) &&
                   (patternOrNull == null || patternOrNull.IsMatch(fromTo));
        }
    }
}