namespace EngineTests.TestModelWithInterfaces
{
    public class CdsTrade : Trade, ICdsTrade
    {
        public ICreditDefaultSwap CdsProduct
        {
            get { return (ICreditDefaultSwap) Product; }

            set { Product = value; }
        }
    }
}