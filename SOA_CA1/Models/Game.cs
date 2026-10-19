namespace SOA_CA1.Models;

public class Game
{
    private List<string> developers = [];
    private List<string> screenshots = [];

    public Game(
        int rawgId,
        string name,
        DateOnly? releaseDate,
        decimal? rating,
        string? artworkUrl,
        string? description)
    {
        RawgId = rawgId;
        Name = name;
        ReleaseDate = releaseDate;
        Rating = rating;
        ArtworkUrl = artworkUrl;
        Description = description;
    }

    public int RawgId { get; }
    public string Name { get; }
    public DateOnly? ReleaseDate { get; }
    public decimal? Rating { get; }
    public string? ArtworkUrl { get; }
    public string? Description { get; }

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