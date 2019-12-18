using System;
using RulesEngine.RulesEngine;

namespace EngineTests
{
    public class DogRules : MappingRules<Dog>
    {
        public DogRules()
        {

            Set(d => d.IsAnimal).With(d => true).OnChanged(d=>d.IsAnimal);

            Set(d=>d.Name).With(d=> "mr. " + d.Name ).If(x=> x.Name != "Clara" && !x.Name.StartsWith("mr.")).OnChanged(d=>d.Name);

            Set(d=>d.IsDangerous).With(d=>d.Age > 3 && d.Name != "Fluffy" ).OnChanged(d=>d.Age);

            Set(d=>d.FavoriteToy).With(d=>GetFavoriteToy()).If(d=>d.FavoriteToy == null).OnChanged(d=>d.FavoriteToy);

        }

        static string GetFavoriteToy()
        {
            return "ball";
        }

        protected override void Trace(Rule<Dog> triggeredRule, string triggerProperty, Dog instance)
        {
            Console.WriteLine(triggeredRule);
            Console.WriteLine($"triggered by {triggerProperty}");
        }
    }
}