﻿using Assets.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            var list3 = new List<int>(4) { 1, 2, 3 };

            // act
            List<int> last1_from_empty = list0.GetLast(1).ToList();
            List<int> last1 = list3.GetLast(1).ToList();
            List<int> last2 = list3.GetLast(2).ToList();
            List<int> last5 = list3.GetLast(5).ToList();

            // assert
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => list3.GetLast(-1));

            Assert.IsTrue(last1_from_empty.Count == 0);

            Assert.IsTrue(last1.Count == 1);
            Assert.IsTrue(last1[0] == 3);

            Assert.IsTrue(last2.Count == 2);
            Assert.IsTrue(last2[0] == 2 
                && last2[1] == 3);

            Assert.IsTrue(last5.Count == 3);
            Assert.IsTrue(last5[0] == 1
                && last5[1] == 2
                && last5[2] == 3);
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

        [Test]
        public void GetAllExceptOne_Test()
        {
            // arrange
            var list = new List<int>(4) { 1, 2, 3, 4, 5 };

            // act
            List<int> ommit1 = list.GetAllExceptOne(0).ToList();
            List<int> ommit3 = list.GetAllExceptOne(2).ToList();
            List<int> ommit5 = list.GetAllExceptOne(4).ToList();

            // assert
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => list.GetAllExceptOne(-1));
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => list.GetAllExceptOne(5));

            Assert.IsTrue(ommit1[0] == 2
                && ommit1[1] == 3
                && ommit1[2] == 4
                && ommit1[3] == 5);
            Assert.IsTrue(ommit3[0] == 1
                && ommit3[1] == 2
                && ommit3[2] == 4
                && ommit3[3] == 5);
            Assert.IsTrue(ommit5[0] == 1
                && ommit5[1] == 2
                && ommit5[2] == 3
                && ommit5[3] == 4);
        }
    }
}
