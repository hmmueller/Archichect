// (c) HMMüller 2006...2015

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Archichect.Transforming.ViolationChecking;

namespace Archichect.Tests {
    [TestClass, ExcludeFromCodeCoverage]
    public class AttributeTests {
        private static readonly string _testAssemblyPath = Path.Combine(Path.GetDirectoryName(typeof(MainTests).Assembly.Location ?? "IGNORE"), "Archichect.TestAssemblyForAttributes.dll");

        [TestMethod]
        public void Exit0() {
            {
                string ruleFile = CreateTempDotNetDepFileName();
                using (TextWriter tw = new StreamWriter(ruleFile)) {
                    tw.Write(@"
                    $ DOTNETITEM ---> DOTNETITEM

                    Archichect.TestAssembly.For.Attributes.** ---> Archichect.TestAssembly.For.Attributes.**
                    Archichect.TestAssembly.For.Attributes ---> System.**

                    $ MY_ITEM_TYPE(NAMESPACE:CLASS:Assembly.Name:Assembly.VERSION:Assembly.CULTURE:MEMBER.Name:MEMBER.SORT:CUSTOM.SectionA:CUSTOM.SectionB:CUSTOM.SectionC) ---> DOTNETITEM
                    // VORERST ------------------------------                    
                    : ---> :

                    $ MY_ITEM_TYPE ---> MY_ITEM_TYPE
                    // VORERST ------------------------------                    
                    : ---> :

                    $ DOTNETASSEMBLY ---> DOTNETASSEMBLY
                    * ---> *
                ");
                }
                Assert.AreEqual(0, Program.Main(
                    new[] {
                        Program.ConfigureOption.Opt,
                        typeof(CheckDeps).Name, "{", CheckDeps.DefaultRuleFileOption.Opt, ruleFile, "}",
                        _testAssemblyPath
                    }
                ));
                File.Delete(ruleFile);
            }
        }

        private static string CreateTempDotNetDepFileName() {
            return Path.GetTempFileName() + ".dll.dep";
        }
    }
}
