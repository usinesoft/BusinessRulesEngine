using System;

namespace RuleEngineTests.TestModelWithInterfaces
{
    public interface ICreditDefaultSwap : IProduct
    {
        DateTime? MaturityDate { get; set; }

        string RefEntity { get; set; }

        string Tenor { get; set; }

        decimal Spread { get; set; }

        decimal Nominal { get; set; }

        string Currency { get; set; }
        string Seniority { get; set; }
        string Restructuring { get; set; }
        string TransactionType { get; set; }
    }
}