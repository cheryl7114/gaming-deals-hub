namespace SOA_CA1.Models;

public class ExchangeRate
{
    public ExchangeRate(
        string fromCurrency,
        string toCurrency,
        decimal rate,
        DateOnly date)
    { 
        FromCurrency = fromCurrency;
        ToCurrency = toCurrency;
        Rate = rate;
        Date = date;
    }

    public string FromCurrency { get; }
    public string ToCurrency { get; }
    public decimal Rate { get; }
    public DateOnly Date { get; }

    public decimal Convert(decimal amount)
    {
        return amount * Rate;
    }
}