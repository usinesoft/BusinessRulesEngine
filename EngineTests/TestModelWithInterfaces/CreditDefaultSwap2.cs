using System;

namespace EngineTests.TestModelWithInterfaces
{
    public class CreditDefaultSwap2:Product2
    {
        public virtual string InstrumentName
        {
            get => "CDS";
        }

        public virtual DateTime? MaturityDate { get; set; }

        public virtual string RefEntity { get; set; }

        public virtual string Tenor { get; set; }

        public virtual decimal Spread { get; set; }

        public virtual decimal Nominal { get; set; }

        public virtual string Currency { get; set; }

        public virtual string Seniority { get; set; }

        public virtual string Restructuring { get; set; }

        public virtual string TransactionType { get; set; }
    }
}