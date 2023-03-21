namespace Movies.Api.Sdk;

public static class ApiEndpoints
{
    private const string ApiBase = "/api";

    public static class Movies
    {
        private const string MoviesBase = $"{ApiBase}/movies";

        public const string Create = MoviesBase;
        public const string GetById = $"{MoviesBase}/{{movieIdOrSlug}}";
        public const string GetAll = MoviesBase;
        public const string Update = $"{MoviesBase}/{{movieId}}";
        public const string Delete = $"{MoviesBase}/{{movieId}}";

        public const string Rate = $"{MoviesBase}/{{movieId}}/ratings";
        public const string DeleteRating = $"{MoviesBase}/{{movieId}}/ratings";
    }

    public static class Ratings
    {
        private const string RatingsBase = $"{ApiBase}/ratings";

        public const string GetUserRatings = $"{RatingsBase}/me";
    }
}