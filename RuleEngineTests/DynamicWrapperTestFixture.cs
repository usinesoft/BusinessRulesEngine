using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using RulesEngine.Interceptors;

namespace RuleEngineTests
{
    [TestFixture]
    public class DynamicWrapperTestFixture
    {
        [Test]
        public void Intercept_changes_with_dynamic_wrapper()
        {
            {
                var instance = new Abcd();

                dynamic abcd = new DynamicWrapper<IAbcd>(instance, new AbcdRules());

                var inotify = (INotifyPropertyChanged) abcd;

                var changed = new List<string>();

                inotify.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

                abcd.A = 1;

                Assert.AreEqual(100, abcd.A);
                Assert.AreEqual(100, instance.A);
                Assert.AreEqual(4, changed.Count);
            }

            {
                var instance = new Bingo();

                dynamic bingo = new DynamicWrapper<IBingo>(instance, new BingoRules());

                var inotify = (INotifyPropertyChanged) bingo;

                var changed = new List<string>();

                inotify.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

                bingo.X = 1;

                Assert.AreEqual("BINGO", bingo.Message);
                Assert.AreEqual(101, instance.X);
                Assert.AreEqual(3, changed.Count);

                bingo.Message = "BONGO";
                Assert.AreEqual("BONGO", bingo.Message);
            }
        }
    }
}