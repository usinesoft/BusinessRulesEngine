using System;

namespace EngineTests.TestModelWithInterfaces
{
    public interface ITrade
    {
        string CounterpartyRole { get; set; }

        DateTime? TradeDate { get; set; }

        DateTime? ValueDate { get; set; }

        string Counterparty { get; set; }

        string ContractId { get; set; }

        string Folder { get; set; }

        string BrokerParty { get; set; }

        string Trader { get; set; }

        string Sales { get; set; }

        string ClearingHouse { get; set; }

        bool MandatoryClearing { get; set; }

        IProduct Product { get; set; }

        decimal SalesCredit { get; set; }
    }
}