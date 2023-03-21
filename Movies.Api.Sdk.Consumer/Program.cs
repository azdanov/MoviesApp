using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

var service = new ServiceCollection();

service
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(provider => new RefitSettings
    {
        AuthorizationHeaderValueGetter = provider.GetRequiredService<AuthTokenProvider>().GetTokenAsync
    })
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:44392"));

var provider = service.BuildServiceProvider();


var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movie = await moviesApi.GetMovieAsync("toy-story-1995");

Console.WriteLine(JsonSerializer.Serialize(movie, new JsonSerializerOptions { WriteIndented = true }));

var request = new GetAllMoviesRequest
{
    Title = null,
    YearOfRelease = null,
    Page = 1,
    PageSize = 10,
    SortBy = "title"
};

var movies = await moviesApi.GetAllMoviesAsync(request);

Console.WriteLine(JsonSerializer.Serialize(movies, new JsonSerializerOptions { WriteIndented = true }));


var createMovieRequest = new CreateMovieRequest
{
    Title = "The Matrix",
    YearOfRelease = 1999,
    Genres = new List<string> { "Action", "Sci-Fi" }
};

var createdMovie = await moviesApi.CreateMovieAsync(createMovieRequest);

Console.WriteLine(JsonSerializer.Serialize(createdMovie, new JsonSerializerOptions { WriteIndented = true }));

var updateMovieRequest = new UpdateMovieRequest
{
    Title = "The Matrix Reloaded",
    YearOfRelease = 2003,
    Genres = new List<string> { "Action", "Sci-Fi" }
};

var updatedMovie = await moviesApi.UpdateMovieAsync(createdMovie.Id.ToString(), updateMovieRequest);

Console.WriteLine(JsonSerializer.Serialize(updatedMovie, new JsonSerializerOptions { WriteIndented = true }));

await moviesApi.DeleteMovieAsync(updatedMovie.Id.ToString());

var deletedMovie = await moviesApi.GetMovieAsync(createdMovie.Id.ToString());

Console.WriteLine(JsonSerializer.Serialize(deletedMovie, new JsonSerializerOptions { WriteIndented = true }));