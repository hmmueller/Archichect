using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Archichect.Matching {
    public abstract class DependencyMatch {
        public string Representation { get; }

        protected DependencyMatch(string representation) {
            Representation = representation;
        }

        public static DependencyMatch Create(string pattern, bool ignoreCase, string arrowTail = "->",
            ItemType usingTypeHint = null, ItemType usedTypeHint = null) {
            int leftEnd = pattern.IndexOf("--", StringComparison.InvariantCulture);
            string left = leftEnd < 0 ? "" : pattern.Substring(0, leftEnd).Trim();
            int rightStart = pattern.LastIndexOf(arrowTail, StringComparison.InvariantCulture);
            string right = rightStart < 0 || rightStart < leftEnd ? "" : pattern.Substring(rightStart + 2).Trim();

            int ldep = leftEnd < 0 ? 0 : leftEnd + 2;
            int rdep = rightStart < 0 || rightStart < leftEnd ? pattern.Length : rightStart;

            if (ldep > rdep) {
                throw new ArgumentException(
                    $"Wrong format of dependecy pattern '{pattern}' (maybe --> instead of --->?)");
            }

            string dep = pattern.Substring(ldep, rdep - ldep).Trim();
            if (Pattern.IsPrefixAndSuffixAsterisksPattern(dep)) {
                var usingPattern = new SingleDependencyMatch(usingTypeHint, dep, "", usedTypeHint, "", ignoreCase);
                var usedPattern = new SingleDependencyMatch(usingTypeHint, dep, "", usedTypeHint, "", ignoreCase);
                return new DependencyMatchDisjunction(pattern, new[] { usingPattern, usedPattern });
            } else {
                return new SingleDependencyMatch(usingTypeHint, left, dep, usedTypeHint, right, ignoreCase);
            }
        }

        public abstract bool IsMatch<TItem>([NotNull] AbstractDependency<TItem> d) where TItem : AbstractItem<TItem>;


        public static readonly string DEPENDENCY_MATCH_HELP = @"
TBD

A dependency match is a string that is matched against dependencies for various
plugins. A dependency match has the following format (unfortunately, not all
plugins follow this format as of today):
   
    [itempattern] -- [dependencypattern] -> [itempattern] 

For the format of item patterns, please see the help text for 'item'.
A dependency pattern has the following format:

    {{countpattern}} [markerpattern]

There are 8 possible count patterns:
    #   count > 0
    ~#  count = 0
    !   bad count > 0
    ~!  bad count = 0
    ?   questionable count > 0
    ~?  questionable count = 0
    =   dependency is a loop (i.e., goes from an item to itself)
    ~=  dependency is not a loop 
The count patterns are combined with a logical 'and'. For example,
    ?~!
matches all dependencies with a questionable count, but no bad count.

The marker pattern is described in the help text for 'marker'.
";
    }

    public class DependencyMatchDisjunction : DependencyMatch {
        private readonly IEnumerable<DependencyMatch> _alternatives;

        public DependencyMatchDisjunction(string representation, IEnumerable<DependencyMatch> alternatives)
            : base(representation) {
            _alternatives = alternatives;
        }

        public override bool IsMatch<TItem>(AbstractDependency<TItem> d) {
            return _alternatives.Any(a => a.IsMatch(d));
        }
    }


    public class SingleDependencyMatch : DependencyMatch {
        [CanBeNull]
        public ItemMatch UsingMatch {
            get;
        }
        [CanBeNull]
        public DependencyPattern DependencyPattern {
            get;
        }
        [CanBeNull]
        public ItemMatch UsedMatch {
            get;
        }

        private static readonly string[] NO_STRINGS = new string[0];

        public SingleDependencyMatch([CanBeNull] ItemMatch usingMatch,
            [CanBeNull] DependencyPattern dependencyPattern, [CanBeNull] ItemMatch usedMatch,
            string representation) : base(representation) {
            UsingMatch = usingMatch;
            DependencyPattern = dependencyPattern;
            UsedMatch = usedMatch;
        }

        public SingleDependencyMatch(ItemType usingTypeHint, string usingPattern, string dependencyPattern, ItemType usedTypeHint, string usedPattern, bool ignoreCase) : this(
            usingPattern != "" ? new ItemMatch(usingTypeHint, usingPattern, 0, ignoreCase, anyWhereMatcherOk: false) : null,
            dependencyPattern != "" ? new DependencyPattern(dependencyPattern, ignoreCase) : null,
            usedPattern != "" ? new ItemMatch(usedTypeHint, usedPattern, usingPattern.Count(c => c == '('), ignoreCase, anyWhereMatcherOk: false) : null,
            usingPattern + "--" + dependencyPattern + "->" + usedPattern) {
        }

        public override bool IsMatch<TItem>([NotNull] AbstractDependency<TItem> d) {
            MatchResult matchLeft = UsingMatch == null ? new MatchResult(true, null) : UsingMatch.Matches(d.UsingItem, NO_STRINGS);
            return matchLeft.Success
                   && (DependencyPattern == null || DependencyPattern.IsMatch(d))
                   && (UsedMatch == null || UsedMatch.Matches(d.UsedItem, matchLeft.Groups).Success);
        }
    }
}