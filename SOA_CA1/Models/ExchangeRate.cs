namespace SOA_CA1.Models;

public class ExchangeRate
{
    private string fromCurrency;
    private string toCurrency;
    private decimal rate;
    private DateOnly date;

    public ExchangeRate(
        string fromCurrency,
        string toCurrency,
        decimal rate,
        DateOnly date)
    { 
        this.fromCurrency = fromCurrency;
        this.toCurrency = toCurrency;
        this.rate = rate;
        this.date = date;
    }

    public string FromCurrency { get; }
    public string ToCurrency { get; }
    public decimal Rate { get; }
    public DateOnly Date { get; }

    public decimal Convert(decimal amount)
    {
        return amount * rate;
    }
}