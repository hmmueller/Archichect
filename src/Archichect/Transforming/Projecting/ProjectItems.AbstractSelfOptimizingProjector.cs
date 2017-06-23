using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Archichect.Transforming.Projecting {
    public partial class ProjectItems {
        public interface IResortableProjectorWithCost : IComparable<IResortableProjectorWithCost>, IProjector {
            double CostPerProjection { get; }

            void ReduceCostCountsInReorganizeToForgetHistory();
        }

        public abstract class AbstractSelfOptimizingProjector<TResortableProjectorWithCost> : AbstractProjector
            where TResortableProjectorWithCost : IResortableProjectorWithCost {
            protected readonly IEqualityComparer<char> _equalityComparer;

            private readonly List<TResortableProjectorWithCost> _projectors;
            private readonly IProjector _fallBackProjector;

            protected readonly int _reorganizeIntervalIncrement;
            protected int _reorganizeInterval;
            private int _stepsToNextReorganize;
            private readonly Stopwatch _timer = new Stopwatch();

            protected AbstractSelfOptimizingProjector(Projection[] orderedProjections, bool ignoreCase, int reorganizeIntervalIncrement, string name)
                : base(name) {
                _stepsToNextReorganize = _reorganizeInterval = _reorganizeIntervalIncrement = reorganizeIntervalIncrement;

                _equalityComparer = ignoreCase
                    ? (IEqualityComparer<char>)new CharIgnoreCaseEqualityComparer()
                    : EqualityComparer<char>.Default;

                // The following is ok if derived projectors do not initialize something in their
                // state which they use in CreateSelectingProjectors ...
                // ReSharper disable once VirtualMemberCallInConstructor 
                _projectors = CreateResortableProjectors(orderedProjections);

                _fallBackProjector = new SimpleProjector(orderedProjections, name: "fallback");
                _timer.Start();
            }

            public IEnumerable<TResortableProjectorWithCost> ProjectorsForTesting => _projectors;

            protected abstract List<TResortableProjectorWithCost> CreateResortableProjectors(Projection[] orderedProjections);

            public override int ProjectCount => _projectors.Sum(p => p.ProjectCount);

            public override int MatchCount => _projectors.Sum(p => p.MatchCount);

            private void Reorganize() {
                _projectors.Sort();
                foreach (var p in _projectors) {
                    p.ReduceCostCountsInReorganizeToForgetHistory();
                }
            }

            public override Item Project(WorkingGraph cachingGraph, Item item, bool left, int dependencyProjectCountForLogging) {
                if (_stepsToNextReorganize-- < 0) {
                    int projectionsPerSecond = (int)(dependencyProjectCountForLogging / _timer.Elapsed.TotalSeconds);
                    if (Log.IsChattyEnabled) {
                        decimal avgMatches = 100m * MatchCount / ProjectCount / 100m;
                        Log.WriteDebug($"{dependencyProjectCountForLogging} dependency projections in {(int)_timer.Elapsed.TotalMilliseconds} ms, i.e. {projectionsPerSecond} proj/s; {avgMatches:F2} matches/proj");
                    }
                    Reorganize();
                    _reorganizeInterval += _reorganizeIntervalIncrement;
                    _stepsToNextReorganize = _reorganizeInterval;
                }
                return ((IProjector)SelectProjector(_projectors, item, left, _stepsToNextReorganize) ?? _fallBackProjector)
                    .Project(cachingGraph, item, left, dependencyProjectCountForLogging);
            }

            [CanBeNull]
            protected abstract TResortableProjectorWithCost SelectProjector(IReadOnlyList<TResortableProjectorWithCost> projectors,
                                                                            Item item, bool left, int stepsToNextReorganize);
        }
    }
}
