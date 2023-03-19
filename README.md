# MoviesApp

A simple RESTful API for movies.

Made with C# / ASP.NET Core 7 and PostgreSQL.
Dapper is used for data access with transactional support.

The docker-compose.yml file containing the database service to get started quickly.

There is a simple Identity project in place to generate a test JWT
token for the API. You'll have to start the Identity project separately from main Api project.

__On initial run, the database will be seeded with some data. So it might be slow.__

---

You can create a user with custom claims. A trusted member can create and update movies.
An admin can delete movies also. Anonymous users can get movies.

A rating for a movie can be added or deleted by any authenticated user. A user can only rate a movie once.

If get movie request is made with a valid token, the user's rating for the movie is included in the response.

---

Versioning is done added by using [ASP.NET API Versioning](https://github.com/dotnet/aspnet-api-versioning).