using System;

namespace RuleEngineTests.TestModelWithoutInterfaces
{
    public class CreditDefaultSwap : IProduct
    {
        public DateTime? MaturityDate { get; set; }

        public string RefEntity { get; set; }

        public string Tenor { get; set; }

        public decimal Spread { get; set; }

        public decimal Nominal { get; set; }

        public string Currency { get; set; }

        public string Seniority { get; set; }

        public string Restructuring { get; set; }

        public string TransactionType { get; set; }
        public string InstrumentName { get; private set; }
    }
}