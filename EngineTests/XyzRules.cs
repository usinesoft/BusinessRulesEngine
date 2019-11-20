using RulesEngine.RulesEngine;

namespace EngineTests
{
    public class XyzRules : MappingRules<Xyz>
    {
        public XyzRules()
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