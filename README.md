# BusinessRulesEngine for .NET applications

## Summary. 
Complex cascade defaulting of properties on an object graph triggered by other property changes

## Introduction

### The problem

I was motivated to develop this component by a real-life problem. Very complex cascading rules that are required by a trade processing system. It is already used in production environment (investment banking). The same need may arrive in a large category of business-line systems.
Given on object graph, when a property of an object is changed, it triggers cascading changes (defaulting of values dependent of other properties) in the whole graph. 
Naïve approaches simply do not work. 
Triggering the business rules inside the setters, or worse, coding the business rules inside the setters. Seems straight-forward but it is not maintainable in the long run:
-	Business rules are strongly coupled with data structure: this does not allow to apply different rules depending on the context
-	Business rules are spread all over the object graph which makes long-term maintenance a nightmare (I really had to do it on a legacy system, it is not hypothetical)
-	Very easy to trigger the same rule multiple times: performance issue and debugging nightmare
-	In most cases the value to be defaulted depends on multiple inputs. There is no guarantee that the code will behave the same way if the inputs are changed in different order. Debugging and maintenance nightmare again.

### The solution

The solution was inspired by the ORM frameworks that need to intercept property changes to generate SQL update commands; a transparent proxy is created around an object, it intercept changes and stores them. That will generate a SQL UPDATE command when the object is saved.

In a similar way, in our case, a transparent proxy will be created around each object in the graph. It intercepts changes and triggers the required rules. 
Let’s see some code:

For start, an abstract example involving a single object:
```csharp
public class Abcd : IAbcd
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
}
```
Rules are expressed in a different class. They are all described in the constructor of this class using a fluent syntax.

 ```csharp
public class AbcdRules : MappingRules<IAbcd>
{
    public AbcdRules()        
    {
        Set(x => x.B)
            .With(x => x.A)
            .If(x => x.A < 100)
            .OnChanged(x => x.A);            

        Set(x => x.C)
            .With(x => x.B)
            .If(x => x.C < 100)
            .OnChanged(x => x.B);            

        Set(x => x.D)
            .With(x => x.C)
            .If(x => x.D < 100)
            .OnChanged(x => x.C);
            

        Set(x => x.A)
            .With(x => x.D + 1)
            .If(x => x.A < 100)
            .OnChanged(x => x.D);
            
    } 

```

In order to use the rules engine we instantiate a "facade". It takes an instance of the object as parameter and an instance of the rules

 ```csharp
var instance = new Abcd();

var rules = new AbcdRules()

var abcd = new InterfaceWrapper<IAbcd>(instance, rules);
```
We set vales on the facade as if it was our business object

Setting one value:

```csharp
abcd.Target.A = 1;
```

Checking the state of the object

```csharp
Assert.AreEqual(100, instance.A);
````

## Intercepting property updates
 
To create a typed proxy around an object, **all public properties need to be virtual** (explicitly declared virtual or inherited from an interface as in the previous example) and the class should **not be sealed**.

An alternative solution is proposed when these conditions cannot be met: using a dynamic proxy.

An example from a real trading system. This time we creata a facade around an object graph containing two nodes trade -> product. 
Setting e property on an object may change the other.

```csharp
var trade = new CdsTrade
{
    Product = new CreditDefaultSwap()
};

var rules = new CdsRules();

dynamic p = new DynamicWrapper<CdsTrade>(trade, rules);

p.CdsProduct.RefEntity = "AXA";

p.Counterparty = "CHASEOTC";

Assert.AreEqual("ICEURO", trade.ClearingHouse);
Assert.AreEqual("MMR", trade.CdsProduct.Restructuring);
Assert.AreEqual("SNR", trade.CdsProduct.Seniority);

```

A small excerpt from the rules of a system in production (the complete code uses around 300 rules):

```csharp
public class CdsRules : MappingRules<CdsTrade>
{
    public CdsRules()        
    {
        Set(t => t.CounterpartyRole)
            .With(t => t.Sales != null ? "Client" : "Dealer")
            .OnChanged(t => t.Sales);            

        Set(t => t.ClearingHouse)
            .With(t => GetDefaultClearingHouse(t.Counterparty, t.CdsProduct.RefEntity))
            .OnChanged(t => t.CdsProduct.RefEntity, t => t.Counterparty);
            
        Set(t => t.SalesCredit)
            .With(t => Calculator(t.CdsProduct.Spread, t.CdsProduct.Nominal))
            .OnChanged(t => t.CdsProduct.Spread, t => t.CdsProduct.RefEntity);
            

        Set(t => t.CdsProduct.TransactionType)
            .With(t => GetTransactionType(t.CdsProduct.RefEntity))
            .OnChanged(t => t.CdsProduct.RefEntity);
            

        Set(t => t.CdsProduct.Currency)
            .With(t => GetDefaultCurrency(t.CdsProduct.TransactionType))
            .OnChanged(t => t.CdsProduct.TransactionType);
            

        Set(t => t.CdsProduct.Restructuring)
            .With(t => GetDefaultRestructuring(t.CdsProduct.TransactionType))
            .OnChanged(t => t.CdsProduct.TransactionType);            

        Set(t => t.CdsProduct.Seniority)
            .With(t => GetDefaultSeniority(t.CdsProduct.TransactionType))
            .OnChanged(t => t.CdsProduct.TransactionType);            
    }
    // more code here    
    ...
}
````

Both facades implement **INotifyPropertyChange** so they can be directly data-bound to a WPF or WindowsForms view.

Internally all property updates are done with code injection (no reflection). As you can see in the performance test, it is blazing fast.


