using BusinessRulesEngine.RulesEngine;

namespace RuleEngineTests
{
    public class AbcdRules : MappingRules<IAbcd>
    {
        public AbcdRules(IAbcd parent)
            : base(parent)
        {
            Set(x => x.B)
                .With(x => x.A)
                .If(x => x.A < 100)
                .OnChanged(x => x.A)
                .EndRule();

            Set(x => x.C)
                .With(x => x.B)
                .If(x => x.C < 100)
                .OnChanged(x => x.B)
                .EndRule();

            Set(x => x.D)
                .With(x => x.C)
                .If(x => x.D < 100)
                .OnChanged(x => x.C)
                .EndRule();

            Set(x => x.A)
                .With(x => x.D + 1)
                .If(x => x.A < 100)
                .OnChanged(x => x.D)
                .EndRule();
        }
    }

    public class XyzRules : MappingRules<Xyz>
    {
        public XyzRules(Xyz parent)
            : base(parent)
        {
            Set(x => x.Y)
                .With(x => x.X * 2)
                .OnChanged(x => x.X)
                .EndRule();

            Set(x => x.Z)
                .With(x => x.Y * 2)
                .OnChanged(x => x.Y)
                .EndRule();


        }
    }
}