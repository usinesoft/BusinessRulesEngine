using System;
using RulesEngine.RulesEngine;

namespace EngineTests
{
    public class BingoRules : MappingRules<IBingo>
    {
        public BingoRules()
        {
            Set(i => i.X)
                .With(i => i.Y + 1)
                .If(i => i.X < 100)
                .OnChanged(i => i.Y);
                

            Set(i => i.Y)
                .With(i => i.X + 1)
                .If(i => i.Y < 100)
                .OnChanged(i => i.X);
                

            Set(i => i.Message)
                .With(i => "BINGO")
                .If(i => i.X >= 100 || i.Y >= 100)
                .OnChanged(i => i.X, i=>i.Y);
                
        }

        #region Overrides of MappingRules<Bingo>

        protected override void Trace(Rule<IBingo> triggeredRule, string triggerProperty, IBingo instance)
        {
            Console.WriteLine("{0, 10} : {1}", triggerProperty, triggeredRule);
        }

        #endregion
    }
}