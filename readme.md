This project is used to demonstrate EF Core logging strategies, covering Query Tags and Log Scopes to maximize logging potential.

## Requirements

* Visual Studio 2022+
* .NET 6+

### SQL Server

The project will by default create a new DB named `TwitterSample` on `.` server.

You can optionally comment the code for Sql Server and use SQLite instead in **Startup.cs** file in `ConfigureServices` method.

## Older versions

Check the [GitHub tags](https://github.com/jernejk/AspNetCoreSerilogExample/tags) for different .NET Core versions from .NET Core 2.2 onwards.
