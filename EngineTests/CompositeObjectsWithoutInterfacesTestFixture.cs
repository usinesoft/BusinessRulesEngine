using EngineTests.TestModelWithoutInterfaces;
using NUnit.Framework;
using RulesEngine.Interceptors;

namespace EngineTests
{
    [TestFixture]
    public class CompositeObjectsWithoutInterfacesTestFixture
    {
        [Test]
        public void Counterparty_change_fills_product()
        {
            var trade = new CdsTrade
            {
                Product = new CreditDefaultSwap()
            };

            dynamic p = new DynamicWrapper<CdsTrade>(trade, new CdsRules());

            p.CdsProduct.RefEntity = "AXA";

            p.Counterparty = "CHASEOTC";

            Assert.AreEqual("ICEURO", trade.ClearingHouse);
            Assert.AreEqual("MMR", trade.CdsProduct.Restructuring);
            Assert.AreEqual("SNR", trade.CdsProduct.Seniority);
        }
    }
}