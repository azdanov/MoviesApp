using System.Data.Common;

namespace Movies.Application.Database;

public interface IDbConnectionFactory
{
    DbDataSource CreateDataSource();
}