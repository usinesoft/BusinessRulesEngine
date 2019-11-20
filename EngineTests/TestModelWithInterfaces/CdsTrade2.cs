namespace EngineTests.TestModelWithInterfaces
{
    public class CdsTrade2 : Trade2
    {
        public virtual CreditDefaultSwap2 CdsProduct
        {
            get => (CreditDefaultSwap2) Product;

            set => Product = value;
        }
    }
}