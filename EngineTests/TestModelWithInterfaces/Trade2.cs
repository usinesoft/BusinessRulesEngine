using System;

namespace EngineTests.TestModelWithInterfaces
{
    public  class Trade2 
    {
        public virtual string CounterpartyRole { get; set; }


        public virtual DateTime? TradeDate { get; set; }

        public virtual DateTime? ValueDate { get; set; }

        public virtual string Counterparty { get; set; }

        public virtual string ContractId { get; set; }

        public virtual string Folder { get; set; }

        public virtual string BrokerParty { get; set; }

        public virtual string Trader { get; set; }

        public virtual string Sales { get; set; }

        public virtual string ClearingHouse { get; set; }

        public virtual bool MandatoryClearing { get; set; }

        public virtual Product2 Product { get; set; }

        public virtual decimal SalesCredit { get; set; }
    }
}