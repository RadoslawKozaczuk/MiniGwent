using NUnit.Framework;
using System.Collections.Generic;
using Assets.Core;
using System.Linq;

namespace Assets.Editor.CoreAssemblyTests
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        class BoolRef
        {
            public bool Value;

            public BoolRef(bool value)
            {
                Value = value;
            }
        }

        [Test]
        public void GetLast_Test()
        {
            // arrange
            var list0 = new List<int>(0);
            var list1 = new List<int>(1) { 1 };
            var list4 = new List<int>(4) { 1, 2, 3, 4 };

            // act
            List<int> last1_from_list0 = list0.GetLast(1).ToList();

            List<int> last0_from_list1 = list1.GetLast(0).ToList();
            List<int> last1_from_list1 = list1.GetLast(1).ToList();
            List<int> last5_from_list1 = list1.GetLast(5).ToList();

            List<int> last5_from_list4 = list4.GetLast(5).ToList();

            // assert
            Assert.IsTrue(last1_from_list0.Count == 0);

            Assert.IsTrue(last0_from_list1.Count == 0);
            Assert.That(last1_from_list1, Has.Exactly(1).EqualTo(1));
            Assert.That(last5_from_list1, Has.Exactly(1).EqualTo(1));

            Assert.IsTrue(last5_from_list4.Count == 4);
            Assert.IsTrue(last5_from_list4[0] == 1 
                && last5_from_list4[1] == 2
                && last5_from_list4[2] == 3
                && last5_from_list4[3] == 4);
        }

        [Test]
        public void AllOnTheRight_Test()
        {
            // arrange
            var list1 = new List<BoolRef>(3) { new BoolRef(false), new BoolRef(false), new BoolRef(false) };
            var list2 = new List<BoolRef>(3) { new BoolRef(false), new BoolRef(false), new BoolRef(false) };
            var list3 = new List<BoolRef>(3) { new BoolRef(false), new BoolRef(false), new BoolRef(false) };

            // act
            list1.AllOnTheRight(0, v => v.Value = true);
            list2.AllOnTheRight(2, v => v.Value = true);
            list3.AllOnTheRight(3, v => v.Value = true);

            // assert
            Assert.IsTrue(list1.All(v => v.Value == true));

            Assert.IsTrue(list2[0].Value == false);
            Assert.IsTrue(list2[1].Value == false);
            Assert.IsTrue(list2[2].Value == true);

            Assert.IsTrue(list3.All(v => v.Value == false));
        }
    }
}
