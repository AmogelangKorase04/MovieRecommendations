using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using MovieRecommendations.Api.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MovieRecommendations.Api.Services
{
    public class MovieService
    {
        private readonly List<Movie> _movies;

        public MovieService()
        {
            _movies = new List<Movie>();

            try
            {
                // Read the entire file content
                var fileContent = File.ReadAllText("Movies.csv");

                // Clean up the data - remove trailing semicolons from each line
                var lines = fileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var cleanedLines = lines.Select(line => line.TrimEnd(';', '\r', '\n')).ToArray();

                // Reconstruct the CSV content
                var cleanedContent = string.Join('\n', cleanedLines);

                // Parse the cleaned CSV
                using var stringReader = new StringReader(cleanedContent);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    // Handle quoted fields properly
                    Quote = '"',
                    Delimiter = ",",
                    // Skip empty records
                    ShouldSkipRecord = args => args.Row.Parser.Record?.All(string.IsNullOrWhiteSpace) == true
                };

                using var csv = new CsvReader(stringReader, config);
                csv.Context.RegisterClassMap<MovieMap>();

                _movies = csv.GetRecords<Movie>().ToList();
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                Console.WriteLine($"Error reading CSV: {ex.Message}");
                // Initialize empty list if CSV reading fails
                _movies = new List<Movie>();
            }
        }

        public IEnumerable<Movie> GetAll() => _movies;
        public Movie? GetByRank(int rank) => _movies.FirstOrDefault(m => m.Rank == rank);
        public IEnumerable<Movie> GetByYear(int year) => _movies.Where(m => m.Year == year);
        public IEnumerable<Movie> GetTopRated(int count = 10) =>
            _movies.OrderByDescending(m => m.Rating).Take(count);
    }

    public class MovieMap : ClassMap<Movie>
    {
        public MovieMap()
        {
            Map(m => m.Rank).Name("Rank").TypeConverter<IntegerConverter>();
            Map(m => m.Year).Name("Year").TypeConverter<IntegerConverter>();
            Map(m => m.Duration).Name("Duration");
            Map(m => m.AgeLimit).Name("AgeLimit");
            Map(m => m.Rating).Name("Rating").TypeConverter<DoubleConverter>();
            Map(m => m.NumberOfRatings).Name("NumberOfRatings").TypeConverter<NumberOfRatingsConverter>();
            Map(m => m.Metascore).Name("Metascore").TypeConverter<NullableIntegerConverter>();
            Map(m => m.Description).Name("Description");
            Map(m => m.Name).Name("Name");
        }
    }

    // Custom converters to handle parsing issues
    public class IntegerConverter : ITypeConverter
    {
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Remove any non-digit characters except minus sign
            var cleanText = new string(text.Where(c => char.IsDigit(c) || c == '-').ToArray());

            if (int.TryParse(cleanText, out var result))
                return result;

            return 0;
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    public class NullableIntegerConverter : ITypeConverter
    {
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // Remove any non-digit characters except minus sign
            var cleanText = new string(text.Where(c => char.IsDigit(c) || c == '-').ToArray());

            if (int.TryParse(cleanText, out var result))
                return result;

            return null;
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    public class DoubleConverter : ITypeConverter
    {
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0.0;

            // Remove any non-digit characters except decimal point and minus sign
            var cleanText = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

            if (double.TryParse(cleanText, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                return result;

            return 0.0;
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    public class NumberOfRatingsConverter : ITypeConverter
    {
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Handle formats like "(2.9M)", "(2M)", "(1.5M)", etc.
            // Remove parentheses and extract the number
            var cleanText = text.Trim('(', ')', ' ');

            // Use regex to extract number and multiplier
            var match = Regex.Match(cleanText, @"^(\d+(?:\.\d+)?)([KMB]?)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                if (double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    var multiplier = match.Groups[2].Value.ToUpper();
                    var multiplierValue = multiplier switch
                    {
                        "K" => 1000,
                        "M" => 1000000,
                        "B" => 1000000000,
                        _ => 1
                    };

                    return (int)(number * multiplierValue);
                }
            }

            // Fallback: extract just digits
            var digitsOnly = new string(text.Where(char.IsDigit).ToArray());
            if (int.TryParse(digitsOnly, out var result))
                return result;

            return 0;
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}