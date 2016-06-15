namespace RuleEngineTests.TestModelWithoutInterfaces
{
    public class CdsTrade : Trade
    {
        public CreditDefaultSwap CdsProduct
        {
            get { return (CreditDefaultSwap) Product; }

            set { Product = value; }
        }
    }
}