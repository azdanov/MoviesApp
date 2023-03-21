# MoviesApp

A simple RESTful API for movies.

Made with C# / ASP.NET Core 7 and PostgreSQL.
Dapper is used for data access.

## Quick Start

```bash
docker-compose up -d
# To start the database.

# API Examples are in api.http file.

dotnet run --project .\Movies.Identity\Movies.Identity.csproj

# This should start the Identity project on http://localhost:5281.
# You can use it to generate a JWT token for the API.

dotnet run --project .\Movies.Api\Movies.Api.csproj

# This should start the Api project on http://localhost:5141
# and you can query the API endpoints.

dotnet run --project .\Movies.Api.Sdk.Consumer\Movies.Api.Sdk.Consumer.csproj

# Run the example console app that uses the SDK to query the API. It automatically fetches a JWT token.
`````

## Notes

The docker-compose.yml file containing the database service to get started quickly.

There is a simple Identity project in place to generate a test JWT
token for the API. You'll have to start the Identity project separately from main Api project.

__On initial run, the database will be seeded with some data. So it might be slow.__

You can create a user with custom claims. A trusted member can create and update movies.
An admin can delete movies also. Anonymous users can get movies.

A rating for a movie can be added or deleted by any authenticated user. A user can only rate a movie once.

If get movie request is made with a valid token, the user's rating for the movie is included in the response.

Versioning is done added by using [ASP.NET API Versioning](https://github.com/dotnet/aspnet-api-versioning).

The movie <-> genre relationship is implemented as one-to-many by duplicating the genre name. Correct way would be to have a separate table that link a movie to a genre.

The are no database transactions at service layer. It's done at the repository layer. One way to do it would be to implement a [Unit of Work pattern](https://stackoverflow.com/a/60565419).