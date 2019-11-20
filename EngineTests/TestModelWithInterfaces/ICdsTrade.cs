namespace EngineTests.TestModelWithInterfaces
{
    public interface ICdsTrade : ITrade
    {
        ICreditDefaultSwap CdsProduct { get; set; }
    }
}