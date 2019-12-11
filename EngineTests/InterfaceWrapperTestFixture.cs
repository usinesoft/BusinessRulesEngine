using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using NUnit.Framework;
using RulesEngine.Interceptors;

namespace EngineTests
{
    [TestFixture]
    public class InterfaceWrapperTestFixture
    {
        [Test]
        public void Intercept_changes_with_interface_wrapper()
        {
            {
                var instance = new Abcd();

                var rules = new AbcdRules();

                Assert.AreEqual(4, rules.RulesCount);

                var abcd = new InterfaceWrapper<IAbcd>(instance, rules);

                var inotify = (INotifyPropertyChanged) abcd;

                var changed = new List<string>();

                inotify.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

                abcd.Target.A = 1;

                Assert.AreEqual(100, abcd.Target.A);
                Assert.AreEqual(100, instance.A);
                Assert.AreEqual(4, changed.Count);
            }

            {
                var instance = new Bingo();

                var bingo = new InterfaceWrapper<IBingo>(instance, new BingoRules());

                var inotify = (INotifyPropertyChanged) bingo;

                var changed = new List<string>();

                inotify.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

                bingo.Target.X = 1;

                Assert.AreEqual("BINGO", bingo.Target.Message);
                Assert.AreEqual(101, instance.X);
                Assert.AreEqual(3, changed.Count);

                bingo.Target.Message = "BONGO";
                Assert.AreEqual("BONGO", bingo.Target.Message);
            }
        }


        [Test]
        public void Intercept_changes_with_interface_wrapper_without_interface()
        {
            {
                var instance = new Xyz();

                var xyz = new InterfaceWrapper<Xyz>(instance, new XyzRules());

                xyz.Target.X = 1; 

                Assert.AreEqual(2, instance.Y);

                Assert.AreEqual(4, instance.Z);
                
            }
        }


        [Test]
        public void Performance_test()
        {
            var rules = new XyzRules();

            // warm up
            {
                var instance = new Xyz();

                var xyz = new InterfaceWrapper<Xyz>(instance, rules);

                xyz.Target.X = 1;

            }

            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < 1000; i++)
            {
                var instance = new Xyz();

                var xyz = new InterfaceWrapper<Xyz>(instance, rules);

                xyz.Target.X = 1;

            }

            sw.Stop();

            Console.WriteLine($"took {sw.ElapsedMilliseconds} ms");
        }
    }

}