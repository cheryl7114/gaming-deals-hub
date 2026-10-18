namespace SOA_CA1.Models;

public class Game
{
    private int rawgId;
    private string name;
    private DateOnly? releaseDate;
    private decimal? rating;
    private string? artworkUrl;
    private string? cheapSharkGameId;
    private string? description;
    private List<string> developers = [];
    private List<string> screenshots = [];

    public Game(
        int rawgId,
        string name,
        DateOnly? releaseDate,
        decimal? rating,
        string? artworkUrl,
        string? description,
        string? cheapSharkGameId = null)
    {
        this.rawgId = rawgId;
        this.name = name;
        this.releaseDate = releaseDate;
        this.rating = rating;
        this.artworkUrl = artworkUrl;
        this.cheapSharkGameId = cheapSharkGameId;
        this.description = description;
    }

    public int RawgId
    {
        get { return rawgId; }
        set { rawgId = value; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public DateOnly? ReleaseDate
    {
        get { return releaseDate; }
        set { releaseDate = value; }
    }

    public decimal? Rating
    {
        get { return rating; }
        set { rating = value; }
    }

    public string? ArtworkUrl
    {
        get { return artworkUrl; }
        set { artworkUrl = value; }
    }

    public string? CheapSharkGameId
    {
        get { return cheapSharkGameId; }
        set { cheapSharkGameId = value; }
    }
    public string? Description
    {
        get { return description; }
        set { description = value; }
    }

    public List<string> Developers
    {
        get { return developers; }
        set { developers = value; }
    }

    public List<string> Screenshots
    {
        get { return screenshots; }
        set { screenshots = value; }
    }
}