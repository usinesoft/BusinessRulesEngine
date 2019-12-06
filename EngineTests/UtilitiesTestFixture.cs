using System;
using System.Diagnostics;
using EngineTests.TestModelWithoutInterfaces;
using NUnit.Framework;
using RulesEngine.Tools;

namespace EngineTests
{
    [TestFixture]
    public class UtilitiesTestFixture
    {
        /// <summary>
        ///     Runs en action multiple times and count the time
        /// </summary>
        /// <param name="action"></param>
        /// <param name="runs"></param>
        /// <returns></returns>
        private long TimeInMilliseconds(Action<int> action, int runs)
        {
            // run once without counter to force JIT
            action(0);
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            try
            {
                for (var i = 0; i < runs; i++)
                {
                    action(i);
                }
            }
            finally
            {
                stopWatch.Stop();
            }

            return stopWatch.ElapsedMilliseconds;
        }

        [Test]
        public void Generate_setter_from_getter_expression()
        {
            var setter = RuntimeCompiler<CdsTrade, string>.SetterFromGetter(cds => cds.CdsProduct.RefEntity);

            var trade = new CdsTrade
            {
                Product = new CreditDefaultSwap()
            };

            setter(trade, "xxx");

            Assert.AreEqual("xxx", trade.CdsProduct.RefEntity);
        }

        [Test]
        public void Get_property_name_from_complex_expression()
        {
            var name = ExpressionTreeHelper.PropertyName((Bingo b) => b.Message.Length);
            Assert.AreEqual("Length", name);

            var fullName = ExpressionTreeHelper.FullPropertyName((Bingo b) => b.Message.Length);
            Assert.AreEqual("Message.Length", fullName);

            var fullName1 = ExpressionTreeHelper.FullPropertyName((Bingo b) => b.Message);
            Assert.AreEqual("Message", fullName1);

            var fullName2 = ExpressionTreeHelper.FullPropertyName((CdsTrade cds) => cds.CdsProduct.RefEntity.Length);
            Assert.AreEqual("CdsProduct.RefEntity.Length", fullName2);
        }

        [Test]
        public void Precompiled_accessors_are_much_faster_then_reflexion_based_ones()
        {
            var property = typeof (Bingo).GetProperty("X");

            var getter = CompiledAccessors.CompiledGetter(typeof (Bingo), "X");
            var setter = CompiledAccessors.CompiledSetter(typeof (Bingo), "X");
            var smartSetter = CompiledAccessors.CompiledSmartSetter(typeof (Bingo), "X");

            var bingo = new Bingo
            {
                X = 3,
                Y = 4
            };

            var time0 = TimeInMilliseconds(i =>
            {
                var before = (int) getter(bingo);
                setter(bingo, before + 1);
            }, 1000000);

            Console.WriteLine("Compiled took {0} ms", time0);

            var time1 = TimeInMilliseconds(i =>
            {
                var before = (int) property.GetValue(bingo);
                property.SetValue(bingo, before + 1);
            }, 1000000);

            Console.WriteLine("Reflexion-based took {0} ms", time1);

            var time2 = TimeInMilliseconds(i => smartSetter(bingo, i), 1000000);
            Console.WriteLine("Compiled smart seter took {0} ms", time2);

            Assert.Less(time2*5, time1); // compiled smart setters should be faster
            Assert.Less(time0*5, time1); // compiled accessors should be much faster than reflexion based ones
        }

        [Test]
        public void Update_an_object_using_precompiled_accessors()
        {
            var bingo = new Bingo
            {
                X = 3,
                Y = 4
            };

            var x = CompiledAccessors.CompiledGetter(typeof (Bingo), "X")(bingo);
            Assert.AreEqual(3, x);

            CompiledAccessors.CompiledSetter(typeof (Bingo), "X")(bingo, 4);
            x = CompiledAccessors.CompiledGetter(typeof (Bingo), "X")(bingo);
            Assert.AreEqual(4, x);

            var smartSetter = CompiledAccessors.CompiledSmartSetter(typeof (Bingo), "Message");
            Assert.IsNotNull(smartSetter);

            // a smart setter sets the property only if value is different
            var changed = smartSetter(bingo, "test");
            Assert.IsTrue(changed);

            changed = smartSetter(bingo, "test");
            Assert.IsFalse(changed);

            changed = smartSetter(bingo, "different");
            Assert.IsTrue(changed);
        }
    }
}