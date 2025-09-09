namespace MovieRecommendations.Api.Models
{
    public class Movie
    {
        public int Rank { get; set; }
        public int Year { get; set; }
        public string? Duration { get; set; }
        public string? AgeLimit { get; set; }
        public double Rating { get; set; }
        public int NumberOfRatings { get; set; }
        public int? Metascore { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
    }
}
