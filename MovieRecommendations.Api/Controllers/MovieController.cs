using Microsoft.AspNetCore.Mvc;
using MovieRecommendations.Api.Models;
using MovieRecommendations.Api.Services;

namespace MovieRecommendations.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieService _movieService;

        public MoviesController(MovieService movieService)
        {
            _movieService = movieService;
        }

        // Existing endpoints
        [HttpGet]
        public IActionResult GetAll() => Ok(_movieService.GetAll());

        [HttpGet("{rank}")]
        public IActionResult GetByRank(int rank)
        {
            var movie = _movieService.GetByRank(rank);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        [HttpGet("year/{year}")]
        public IActionResult GetByYear(int year) => Ok(_movieService.GetByYear(year));

        [HttpGet("top/{count?}")]
        public IActionResult GetTopRated(int count = 10) => Ok(_movieService.GetTopRated(count));

        // New filtering endpoints
        [HttpGet("filter/rating")]
        public IActionResult FilterByRating([FromQuery] double minRating = 0, [FromQuery] double maxRating = 10)
        {
            var movies = _movieService.GetAll()
                .Where(m => m.Rating >= minRating && m.Rating <= maxRating)
                .OrderByDescending(m => m.Rating);
            return Ok(movies);
        }

        [HttpGet("filter/year-range")]
        public IActionResult FilterByYearRange([FromQuery] int startYear, [FromQuery] int endYear)
        {
            var movies = _movieService.GetAll()
                .Where(m => m.Year >= startYear && m.Year <= endYear)
                .OrderBy(m => m.Year);
            return Ok(movies);
        }

        [HttpGet("filter/duration")]
        public IActionResult FilterByDuration([FromQuery] int maxMinutes = 180)
        {
            var movies = _movieService.GetAll()
                .Where(m => ParseDurationToMinutes(m.Duration) <= maxMinutes)
                .OrderBy(m => ParseDurationToMinutes(m.Duration));
            return Ok(movies);
        }

        [HttpGet("filter/age-rating/{ageLimit}")]
        public IActionResult FilterByAgeRating(string ageLimit)
        {
            var movies = _movieService.GetAll()
                .Where(m => m.AgeLimit?.Equals(ageLimit, StringComparison.OrdinalIgnoreCase) == true);
            return Ok(movies);
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required");

            var movies = _movieService.GetAll()
                .Where(m =>
                    (m.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                    (m.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true))
                .OrderByDescending(m => m.Rating);

            return Ok(movies);
        }

        [HttpGet("popular")]
        public IActionResult GetMostPopular([FromQuery] int count = 10)
        {
            var movies = _movieService.GetAll()
                .OrderByDescending(m => m.NumberOfRatings)
                .Take(count);
            return Ok(movies);
        }

        [HttpGet("critically-acclaimed")]
        public IActionResult GetCriticallyAcclaimed([FromQuery] int minMetascore = 80)
        {
            var movies = _movieService.GetAll()
                .Where(m => m.Metascore.HasValue && m.Metascore.Value >= minMetascore)
                .OrderByDescending(m => m.Metascore);
            return Ok(movies);
        }

        [HttpGet("family-friendly")]
        public IActionResult GetFamilyFriendly()
        {
            var familyRatings = new[] { "U", "PG", "G" };
            var movies = _movieService.GetAll()
                .Where(m => familyRatings.Contains(m.AgeLimit))
                .OrderByDescending(m => m.Rating);
            return Ok(movies);
        }

        [HttpGet("quick-watch")]
        public IActionResult GetQuickWatch([FromQuery] int maxMinutes = 120)
        {
            var movies = _movieService.GetAll()
                .Where(m => ParseDurationToMinutes(m.Duration) <= maxMinutes)
                .OrderByDescending(m => m.Rating)
                .Take(20);
            return Ok(movies);
        }

        private static int ParseDurationToMinutes(string? duration)
        {
            if (string.IsNullOrWhiteSpace(duration)) return 0;

            // Parse "2h 22m" format
            var hours = 0;
            var minutes = 0;

            if (duration.Contains('h'))
            {
                var hoursPart = duration.Split('h')[0].Trim();
                int.TryParse(hoursPart, out hours);
            }

            if (duration.Contains('m'))
            {
                var minutesPart = duration.Split('h').LastOrDefault()?.Replace("m", "").Trim();
                int.TryParse(minutesPart, out minutes);
            }

            return (hours * 60) + minutes;
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly MovieService _movieService;

        public AnalyticsController(MovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var movies = _movieService.GetAll().ToList();

            var stats = new
            {
                TotalMovies = movies.Count,
                AverageRating = movies.Average(m => m.Rating),
                YearRange = new { Min = movies.Min(m => m.Year), Max = movies.Max(m => m.Year) },
                MostPopular = movies.OrderByDescending(m => m.NumberOfRatings).First().Name,
                HighestRated = movies.OrderByDescending(m => m.Rating).First().Name,
                RatingDistribution = movies
                    .GroupBy(m => Math.Floor(m.Rating))
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Ok(stats);
        }

        [HttpGet("by-decade")]
        public IActionResult GetByDecade()
        {
            var moviesByDecade = _movieService.GetAll()
                .GroupBy(m => (m.Year / 10) * 10)
                .Select(g => new
                {
                    Decade = $"{g.Key}s",
                    Count = g.Count(),
                    AverageRating = g.Average(m => m.Rating),
                    TopMovie = g.OrderByDescending(m => m.Rating).First().Name
                })
                .OrderBy(x => x.Decade);

            return Ok(moviesByDecade);
        }

        [HttpGet("rating-vs-popularity")]
        public IActionResult GetRatingVsPopularity()
        {
            var analysis = _movieService.GetAll()
                .Select(m => new
                {
                    m.Name,
                    m.Rating,
                    Popularity = m.NumberOfRatings,
                    PopularityScore = CalculatePopularityScore(m.NumberOfRatings),
                    BalancedScore = (m.Rating * 0.7) + (CalculatePopularityScore(m.NumberOfRatings) * 0.3)
                })
                .OrderByDescending(x => x.BalancedScore);

            return Ok(analysis);
        }

        private static double CalculatePopularityScore(int numberOfRatings)
        {
            // Normalize popularity to 0-10 scale (assuming max is around 3M)
            return Math.Min(10.0, (numberOfRatings / 300000.0));
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly MovieService _movieService;

        public RecommendationsController(MovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("similar-year/{movieId}")]
        public IActionResult GetSimilarByYear(int movieId, [FromQuery] int count = 5)
        {
            var movie = _movieService.GetByRank(movieId);
            if (movie == null) return NotFound();

            var similarMovies = _movieService.GetAll()
                .Where(m => m.Rank != movieId && Math.Abs(m.Year - movie.Year) <= 5)
                .OrderByDescending(m => m.Rating)
                .Take(count);

            return Ok(similarMovies);
        }

        [HttpGet("hidden-gems")]
        public IActionResult GetHiddenGems([FromQuery] int count = 10)
        {
            // High rating but relatively fewer ratings (hidden gems)
            var gems = _movieService.GetAll()
                .Where(m => m.Rating >= 8.5 && m.NumberOfRatings < 1000000)
                .OrderByDescending(m => m.Rating)
                .Take(count);

            return Ok(gems);
        }

        [HttpGet("crowd-pleasers")]
        public IActionResult GetCrowdPleasers([FromQuery] int count = 10)
        {
            // High rating AND high popularity
            var crowdPleasers = _movieService.GetAll()
                .Where(m => m.Rating >= 8.5 && m.NumberOfRatings >= 1500000)
                .OrderByDescending(m => m.Rating)
                .Take(count);

            return Ok(crowdPleasers);
        }

        [HttpGet("time-based")]
        public IActionResult GetTimeBasedRecommendations([FromQuery] int availableMinutes = 120)
        {
            var movies = _movieService.GetAll()
                .Where(m => ParseDurationToMinutes(m.Duration) <= availableMinutes)
                .OrderByDescending(m => m.Rating)
                .Take(10);

            return Ok(movies);
        }

        private static int ParseDurationToMinutes(string? duration)
        {
            if (string.IsNullOrWhiteSpace(duration)) return 0;

            var hours = 0;
            var minutes = 0;

            if (duration.Contains('h'))
            {
                var hoursPart = duration.Split('h')[0].Trim();
                int.TryParse(hoursPart, out hours);
            }

            if (duration.Contains('m'))
            {
                var minutesPart = duration.Split('h').LastOrDefault()?.Replace("m", "").Trim();
                int.TryParse(minutesPart, out minutes);
            }

            return (hours * 60) + minutes;
        }
    }
}