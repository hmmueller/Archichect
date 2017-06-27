using System;
using System.Collections.Generic;
using System.Linq;
using Archichect.Immutables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Archichect.Tests {
    public interface IGrandparent {
        string FixedName { get; }
        int Variable { get; }
        ReadOnlyParent Parent { get; }
    }

    public class ReadOnlyGrandparent : Immutable<ReadOnlyGrandparent, ReadWriteGrandparent>, IGrandparent {
        public string FixedName { get; }
        public int Variable { get; }
        public ReadOnlyParent Parent { get; }

        public ReadOnlyGrandparent(string fixedName, int variable, ReadOnlyParent parent) {
            FixedName = fixedName;
            Variable = variable;
            Parent = parent;
        }

        protected override ReadWriteGrandparent CreateMutable() {
            return new ReadWriteGrandparent(FixedName, Variable, Parent);
        }

        //public static IEnumerable<ReadOnlyGrandparent> SomeAlgorithm(IEnumerable<ReadOnlyGrandparent> input,
        //    Func<ReadOnlyGrandparent, string> parentSelector, Func<ReadOnlyParent, bool> parentSelector,
        //    Func<ReadOnlyParent, bool> removeParentSelector, ImmutableSupport support) {
        //    foreach (var p in input) {
        //        string markerToAdd = parentSelector(p);
        //        if (markerToAdd != null) {
        //            //p.Variable++;
        //            p.GetOrCreateMutable(support).Variable++;
        //        }
        //        if (parentSelector(p.Parent)) {
        //            p.Parent.GetOrCreateMutable(support).Variable++;
        //        }
        //        foreach (var c in p.Parentren.Where(c => parentSelector(c))) {
        //            c.GetOrCreateMutable(support).Variable++;
        //        }
        //        p.GetOrCreateMutable(support).Parentren.RemoveAll(c => removeParentSelector(c));
        //    }
        //
        //    return input.Immutify<ReadOnlyGrandparent, ReadWriteGrandparent>(support);
        //}

        protected override ReadOnlyGrandparent Immutify(ReadWriteGrandparent mutable, ImmutableSupport support) {
            IGrandparent source = mutable ?? (IGrandparent)this;
            int variable = source.Variable;
            ReadOnlyParent parent = support.Immutify<ReadOnlyParent, ReadWriteParent>(Parent);

            return Equals(variable, Variable) && Equals(parent, Parent)
                ? this
                : new ReadOnlyGrandparent(FixedName, variable, parent);
        }
    }

    public class ReadWriteGrandparent : Mutable<ReadOnlyGrandparent, ReadWriteGrandparent>, IGrandparent {
        public ReadWriteGrandparent(string fixedName, int variable, ReadOnlyParent parent) {
            FixedName = fixedName;
            Variable = variable;
            Parent = parent;
        }

        public string FixedName { get; }
        public int Variable { get; set; }
        public ReadOnlyParent Parent { get; set; }
    }

    public interface IParent {
        string FixedName { get; }
        int Variable { get; }
        SharedReadOnlyChild SharedChild { get; }
        ReadOnlyChild Child { get; }
        IEnumerable<ReadOnlyChild> Children { get; }
    }

    public class ReadOnlyParent : Immutable<ReadOnlyParent, ReadWriteParent>, IParent {
        public string FixedName { get; }
        public int Variable { get; }
        public SharedReadOnlyChild SharedChild { get; }
        public ReadOnlyChild Child { get; }
        public IEnumerable<ReadOnlyChild> Children { get; }

        public ReadOnlyParent(string fixedName, int variable, SharedReadOnlyChild sharedChild,
                              ReadOnlyChild child, IEnumerable<ReadOnlyChild> children) {
            FixedName = fixedName;
            Variable = variable;
            SharedChild = sharedChild;
            Child = child;
            Children = children.ToArray();
        }

        protected override ReadWriteParent CreateMutable() {
            return new ReadWriteParent(FixedName, Variable, SharedChild, Child, Children);
        }

        public static IEnumerable<ReadOnlyParent> SomeAlgorithm(IEnumerable<ReadOnlyParent> input,
            Func<ReadOnlyParent, string> parentSelector, Func<ReadOnlyChild, bool> childSelector,
            Func<ReadOnlyChild, bool> removeChildSelector, ImmutableSupport support) {
            foreach (var p in input) {
                string markerToAdd = parentSelector(p);
                if (markerToAdd != null) {
                    //p.Variable++;
                    p.GetOrCreateMutable(support).Variable++;
                    p.GetOrCreateMutable(support).SharedChild.GetOrCreateMutable(support).Data.Add(markerToAdd);
                }
                if (childSelector(p.Child)) {
                    p.Child.GetOrCreateMutable(support).Variable++;
                }
                foreach (var c in p.Children.Where(c => childSelector(c))) {
                    c.GetOrCreateMutable(support).Variable++;
                }
                p.GetOrCreateMutable(support).ChildrenList.RemoveAll(c => removeChildSelector(c));
            }

            return support.Immutify<ReadOnlyParent, ReadWriteParent>(input);
        }

        protected override ReadOnlyParent Immutify(ReadWriteParent mutable, ImmutableSupport support) {
            IParent source = mutable ?? (IParent)this;

            int variable = source.Variable;
            SharedReadOnlyChild sharedChild = support.Immutify<SharedReadOnlyChild, SharedWriteableChild>(SharedChild);
            ReadOnlyChild child = support.Immutify<ReadOnlyChild, ReadWriteChild>(Child);
            IEnumerable<ReadOnlyChild> children = support.Immutify<ReadOnlyChild, ReadWriteChild>(Children);

            // TODO: Equals für die Collections? ... gehört womöglich in Extensions!
            return Equals(variable, Variable) && Equals(sharedChild, SharedChild) && Equals(child, Child) && Equals(children, Children)
                ? this
                : new ReadOnlyParent(FixedName, variable, sharedChild, child, children);
        }
    }

    public class ReadWriteParent : Mutable<ReadOnlyParent, ReadWriteParent>, IParent {
        public ReadWriteParent(string fixedName, int variable, SharedReadOnlyChild sharedChild,
            ReadOnlyChild child, IEnumerable<ReadOnlyChild> children) {
            FixedName = fixedName;
            Variable = variable;
            SharedChild = sharedChild;
            Child = child;
            ChildrenList = children.ToList();
        }

        public SharedReadOnlyChild SharedChild { get; set; }
        public string FixedName { get; }
        public int Variable { get; set; }
        public ReadOnlyChild Child { get; set; }
        public List<ReadOnlyChild> ChildrenList { get; }
        public IEnumerable<ReadOnlyChild> Children => ChildrenList;
    }

    public class SharedReadOnlyChild : Immutable<SharedReadOnlyChild, SharedWriteableChild> {
        private readonly Dictionary<SharedReadOnlyChild, SharedReadOnlyChild> _cache;
        private readonly HashSet<string> _data;

        private SharedReadOnlyChild(HashSet<string> data, Dictionary<SharedReadOnlyChild, SharedReadOnlyChild> cache) {
            _cache = cache;
            _data = new HashSet<string>(data); // TODO: explain - other variant: Make new HashSet() in Immutify only on demand
        }

        protected override SharedWriteableChild CreateMutable() {
            return new SharedWriteableChild(_data);
        }

        protected override SharedReadOnlyChild Immutify(SharedWriteableChild mutable, ImmutableSupport support) {
            if (mutable == null) {
                return this;
            } else if (mutable.Data.SetEquals(_data)) {
                return this;
            } else {
                SharedReadOnlyChild key = new SharedReadOnlyChild(mutable.Data, _cache);
                SharedReadOnlyChild result;
                if (!_cache.TryGetValue(key, out result)) {
                    _cache.Add(key, result = key);
                }
                return result;
            }
        }
    }

    public class SharedWriteableChild : Mutable<SharedReadOnlyChild, SharedWriteableChild> {
        public HashSet<string> Data;

        public SharedWriteableChild(HashSet<string> data) {
            Data = data;
        }
    }

    public class ReadOnlyChild : Immutable<ReadOnlyChild, ReadWriteChild> {
        public ReadOnlyChild(string fixedName, int variable) {
            FixedName = fixedName;
            Variable = variable;
        }

        public string FixedName { get; }
        public int Variable { get; }
        protected override ReadWriteChild CreateMutable() {
            return new ReadWriteChild(FixedName, Variable);
        }

        protected override ReadOnlyChild Immutify(ReadWriteChild mutable, ImmutableSupport support) {
            if (mutable == null) {
                return this;
            } else {
                int variable = mutable.Variable;

                return Equals(variable, Variable)
                    ? this
                    : new ReadOnlyChild(FixedName, variable);
            }
        }
    }

    public class ReadWriteChild : Mutable<ReadOnlyChild, ReadWriteChild> {
        public ReadWriteChild(string fixedName, int variable) {
            FixedName = fixedName;
            Variable = variable;
        }

        public string FixedName { get; }
        public int Variable { get; set; }
    }

    [TestClass]
    public class TestImmutableMutable {
        [TestMethod]
        public void ParentWithChangingChild() {
            var p = new ReadOnlyParent("p", 1000, null, null, new ReadOnlyChild[0]);
            var gp = new ReadOnlyGrandparent("g", 100, p);

            var s = new ImmutableSupport();

            p.GetOrCreateMutable(s).Variable++;

            ReadOnlyGrandparent gp2 = s.Immutify<ReadOnlyGrandparent, ReadWriteGrandparent>(gp);
            Assert.AreEqual(1000, p.Variable);
            Assert.AreEqual(100, gp.Variable);
            Assert.AreEqual(1001, gp2.Parent.Variable);
            Assert.AreEqual(100, gp2.Variable);
            Assert.AreNotSame(gp, gp2);
        }
        [TestMethod]
        public void ParentWithNonchangingChild() {
            var p = new ReadOnlyParent("p", 1000, null, null, new ReadOnlyChild[0]);
            var gp = new ReadOnlyGrandparent("g", 100, p);

            var s = new ImmutableSupport();

            p.GetOrCreateMutable(s).Variable = p.Variable;

            ReadOnlyGrandparent gp2 = s.Immutify<ReadOnlyGrandparent, ReadWriteGrandparent>(gp);
            Assert.AreSame(gp, gp2);
            Assert.AreEqual(1000, p.Variable);
            Assert.AreEqual(100, gp.Variable);
        }
    }
}
