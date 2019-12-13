using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using EngineTests.TestModelWithInterfaces;
using NUnit.Framework;
using RulesEngine.RulesEngine.Explain;

namespace EngineTests
{

    [TestFixture]
    public class ExplainTestFixture
    {


        class Dog
        {
            public string Name { get; set; }
            public int Age { get; set; }


        }

        /// <summary>
        /// This function is never called 
        /// </summary>
        /// <param name="abcd"></param>
        /// <returns></returns>
        bool SomeComplicatedCheck(Abcd abcd)
        {
            throw new NotImplementedException();
        }


        int DummyComputer(Abcd parent)
        {
            return 1;
        }

        [Test]
        public void ExplainSet()
        {
            {
                Expression<Func<Abcd, int>> expr = x => DummyComputer(x);

                Console.WriteLine(expr.TryExplain());
            }
            
            {
                Expression<Func<Abcd, int>> expr = x => 44;

                Console.WriteLine(expr.TryExplain());
            }

            {
                Expression<Func<Abcd, int>> expr = x => x.A;

                Console.WriteLine(expr.TryExplain());
            }

            {
                Expression<Func<Abcd, int>> expr = x => x.A + 1;

                Console.WriteLine(expr.TryExplain());
            }
        }


        [Test]
        public void ExplainIf()
        {

            {
                Expression<Func<Abcd, bool>> expr = x => x.A > 10;

                Console.WriteLine( expr.TryExplain());
            }

            {
                Expression<Func<Abcd, bool>> expr = x => x.A > 10 && x.B == 3;

                Console.WriteLine(expr.TryExplain());
            }

            {
                Expression<Func<Abcd, bool>> expr = x => x.A > 10 && x.B == 3 || x.C >= 0;

                Console.WriteLine(expr.TryExplain());
            }

            {
                var list = new[] {12, 13, 14};
                Expression<Func<Abcd, bool>> expr = x => list.Contains(x.A);

                Console.WriteLine(expr.TryExplain());
            }

            {
                var list = new[] { 12, 13, 14 };
                Expression<Func<Abcd, bool>> expr = x => list.Contains(x.A) || x.B == 1;

                Console.WriteLine(expr.TryExplain());
            }

            {
                var list = new[] { 12, 13, 14 };
                Expression<Func<Abcd, bool>> expr = x => list.Contains(x.A) && x.B == 1;

                Console.WriteLine(expr.TryExplain());
            }

            {
                var listNames = new[] { "Fluffy", "Puffy" };
                var listAges = new[] { 1, 2, 3 };

                Expression<Func<Dog, bool>> expr = x => listNames.Contains(x.Name) && listAges.Contains(x.Age);

                Console.WriteLine(expr.TryExplain());
            }

            {
                var listNames = new[] { "Fluffy", "Puffy" };
                var listAges = new[] { 1, 2, 3 };

                Expression<Func<Dog, bool>> expr = x => listNames.Contains(x.Name) || listAges.Contains(x.Age);

                Console.WriteLine(expr.TryExplain());
            }

            {
               
                Expression<Func<Dog, bool>> expr = x => x.Name.StartsWith("Flu") || x.Name.StartsWith("Plu");

                Console.WriteLine(expr.TryExplain());
            }

            {

                Expression<Func<Dog, bool>> expr = x => x.Name.StartsWith("Flu") && x.Name.EndsWith("ffy");

                Console.WriteLine(expr.TryExplain());
            }


            {
                Expression<Func<Abcd, bool>> expr = x => SomeComplicatedCheck(x);

                Console.WriteLine(expr.TryExplain());
            }
        }



        [Test]
        public void ExplainRules()
        {
            {
                var rules = new AbcdRules();

                foreach (var rule in rules.Rules)
                {
                    Console.WriteLine(rule);
                }
            }

            Console.WriteLine();

            {
                var rules = new DogRules();

                foreach (var rule in rules.Rules)
                {
                    Console.WriteLine(rule);
                }
            }

            Console.WriteLine();

            {
                var rules = new CdsRules();

                foreach (var rule in rules.Rules)
                {
                    Console.WriteLine(rule);
                }
            }
        }



    }
}
