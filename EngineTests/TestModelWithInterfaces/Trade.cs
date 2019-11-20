using System;

namespace EngineTests.TestModelWithInterfaces
{
    public class Trade : ITrade
    {
        public string CounterpartyRole { get; set; }


        public DateTime? TradeDate { get; set; }

        public DateTime? ValueDate { get; set; }

        public string Counterparty { get; set; }

        public string ContractId { get; set; }

        public string Folder { get; set; }

        public string BrokerParty { get; set; }

        public string Trader { get; set; }

        public string Sales { get; set; }

        public string ClearingHouse { get; set; }

        public bool MandatoryClearing { get; set; }

        public IProduct Product { get; set; }

        public decimal SalesCredit { get; set; }
    }
}