using System.Threading.Tasks;
using EngineTests.TestModelWithInterfaces;
using NUnit.Framework;
using RulesEngine.Interceptors;

namespace EngineTests
{
    [TestFixture]
    public class CompositeObjectsWithInterfacesTestFixture
    {

        [Test]
        public void Multi_threaded_test()
        {
            Parallel.For(0, 1000, (i) => Counterparty_change_fills_product());
        }

        [Test]
        public void Counterparty_change_fills_product()
        {
            var trade = new CdsTrade
            {
                Product = new CreditDefaultSwap()
            };

            var p = new InterfaceWrapper<ICdsTrade>(trade, new CdsRules()).Target;

            p.CdsProduct.RefEntity = "AXA";

            p.Counterparty = "CHASEOTC";

            Assert.AreEqual("ICEURO", trade.ClearingHouse);
            Assert.AreEqual("MMR", trade.CdsProduct.Restructuring);
            Assert.AreEqual("SNR", trade.CdsProduct.Seniority);

        }

        [Test]
        public void Counterparty_change_fills_product2()
        {
            var trade = new CdsTrade2
            {
                Product = new CreditDefaultSwap2()
            };

            var p = new InterfaceWrapper<CdsTrade2>(trade, new CdsRules2()).Target;

            p.CdsProduct.RefEntity = "AXA";

            p.Counterparty = "CHASEOTC";

            Assert.AreEqual("ICEURO", trade.ClearingHouse);
            Assert.AreEqual("MMR", trade.CdsProduct.Restructuring);
            Assert.AreEqual("SNR", trade.CdsProduct.Seniority);

        }
    }
}