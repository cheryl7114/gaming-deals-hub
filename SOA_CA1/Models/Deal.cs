namespace SOA_CA1.Models;

public class Deal
{
    private string dealId;
    private string cheapSharkGameId;
    private string storeId;
    private string storeName;
    private decimal salePriceUsd;
    private decimal normalPriceUsd;
    private string? dealUrl;

    public Deal(
        string dealId,
        string cheapSharkGameId,
        string storeId,
        string storeName,
        decimal salePriceUsd,
        decimal normalPriceUsd,
        string? dealUrl)
    {
        this.dealId = dealId;
        this.cheapSharkGameId = cheapSharkGameId;
        this.storeId = storeId;
        this.storeName = storeName;
        this.salePriceUsd = salePriceUsd;
        this.normalPriceUsd = normalPriceUsd;
        this.dealUrl = dealUrl;
    }

    public string DealId
    {
        get { return dealId; }
        set { dealId = value; }
    }

    public string CheapSharkGameId
    {
        get { return cheapSharkGameId; }
        set { cheapSharkGameId = value; }
    }

    public string StoreId
    {
        get { return storeId; }
        set { storeId = value; }
    }

    public string StoreName
    {
        get { return storeName; }
        set { storeName = value; }
    }

    public decimal SalePriceUsd
    {
        get { return salePriceUsd; }
        set { salePriceUsd = value; }
    }

    public decimal NormalPriceUsd
    {
        get { return normalPriceUsd; }
        set { normalPriceUsd = value; }
    }

    public string? DealUrl
    {
        get { return dealUrl; }
        set { dealUrl = value; }
    }

    public decimal? SavingsPercent
    {
        get
        {
            if (normalPriceUsd <= 0)
            {
                return null;
            }

            return (normalPriceUsd - salePriceUsd) / normalPriceUsd * 100;
        }
    }
}