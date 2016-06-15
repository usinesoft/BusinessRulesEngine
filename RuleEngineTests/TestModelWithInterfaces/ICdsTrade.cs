namespace RuleEngineTests.TestModelWithInterfaces
{
    public interface ICdsTrade : ITrade
    {
        ICreditDefaultSwap CdsProduct { get; set; }
    }
}