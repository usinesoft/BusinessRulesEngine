namespace EngineTests.TestModelWithoutInterfaces
{
    public class CdsTrade : Trade
    {
        public CreditDefaultSwap CdsProduct
        {
            get => (CreditDefaultSwap) Product;

            set => Product = value;
        }
    }
}