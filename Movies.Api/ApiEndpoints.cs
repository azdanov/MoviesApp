namespace Movies.Api;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class V1
    {
        private const string VersionBase = $"{ApiBase}/v1";

        public static class Movies
        {
            private const string MoviesBase = $"{VersionBase}/movies";

            public const string Create = MoviesBase;
            public const string GetById = $"{MoviesBase}/{{movieIdOrSlug}}";
            public const string GetAll = MoviesBase;
            public const string Update = $"{MoviesBase}/{{movieId:guid}}";
            public const string Delete = $"{MoviesBase}/{{movieId:guid}}";

            public const string Rate = $"{MoviesBase}/{{movieId:guid}}/ratings";
            public const string DeleteRating = $"{MoviesBase}/{{movieId:guid}}/ratings";
        }

        public static class Ratings
        {
            private const string RatingsBase = $"{VersionBase}/ratings";

            public const string GetUserRatings = $"{RatingsBase}/me";
        }
    }

    public static class V2
    {
        private const string VersionBase = $"{ApiBase}/v1";

        public static class Movies
        {
            private const string MoviesBase = $"{VersionBase}/movies";

            public const string Create = MoviesBase;
            public const string GetById = $"{MoviesBase}/{{movieIdOrSlug}}";
            public const string GetAll = MoviesBase;
            public const string Update = $"{MoviesBase}/{{movieId:guid}}";
            public const string Delete = $"{MoviesBase}/{{movieId:guid}}";

            public const string Rate = $"{MoviesBase}/{{movieId:guid}}/ratings";
            public const string DeleteRating = $"{MoviesBase}/{{movieId:guid}}/ratings";
        }

        public static class Ratings
        {
            private const string RatingsBase = $"{VersionBase}/ratings";

            public const string GetUserRatings = $"{RatingsBase}/me";
        }
    }
}