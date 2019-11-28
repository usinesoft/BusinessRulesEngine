using RulesEngine.RulesEngine;

namespace EngineTests
{
    public class AbcdRules : MappingRules<IAbcd>
    {
        public AbcdRules()
        {
            Set(x => x.B)
                .With(x => x.A)
                .If(x => x.A < 100)
                .OnChanged(x => x.A);


            Set(x => x.C)
                .With(x => x.B)
                .If(x => x.C < 100)
                .OnChanged(x => x.B);


            Set(x => x.D)
                .With(x => x.C)
                .If(x => x.D < 100)
                .OnChanged(x => x.C);


            Set(x => x.A)
                .With(x => x.D + 1)
                .If(x => x.A < 100)
                .OnChanged(x => x.D);

        }
    }
}