namespace SOA_CA1.Models;

public class Deal
{
    public Deal(
        string storeName,
        decimal salePriceUsd,
        decimal normalPriceUsd,
        string? dealUrl)
    {
        StoreName = storeName;
        SalePriceUsd = salePriceUsd;
        NormalPriceUsd = normalPriceUsd;
        DealUrl = dealUrl;
    }
    
    public string StoreName { get; }
    public decimal SalePriceUsd { get; }
    public decimal NormalPriceUsd { get; }
    public string? DealUrl { get; }
}