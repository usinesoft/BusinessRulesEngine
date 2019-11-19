using System;
using RulesEngine.RulesEngine;

namespace RuleEngineTests
{
    public class BingoRules : MappingRules<IBingo>
    {
        public BingoRules()
        {
            Set(i => i.X)
                .With(i => i.Y + 1)
                .OnChanged(i => i.Y)
                .If(i => i.X < 100)
                .EndRule();

            Set(i => i.Y)
                .With(i => i.X + 1)
                .OnChanged(i => i.X)
                .If(i => i.Y < 100)
                .EndRule();

            Set(i => i.Message)
                .With(i => "BINGO")
                .If(i => i.X >= 100 || i.Y >= 100)
                .OnChanged(i => i.X)
                .Or(i => i.Y)
                .EndRule();
        }

        #region Overrides of MappingRules<Bingo>

        protected override void Trace(Rule<IBingo> triggeredRule, string triggerProperty, IBingo instance)
        {
            Console.WriteLine("{0, 10} : {1}", triggerProperty, triggeredRule);
        }

        #endregion
    }
}