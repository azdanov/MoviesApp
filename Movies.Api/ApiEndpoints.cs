namespace Movies.Api;

public static class ApiEndpoints
{
    private const string Api = "api";

    public static class Movies
    {
        private const string Base = $"{Api}/movies";

        public const string Create = Base;
        public const string GetById = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
    }
}