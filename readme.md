This project is used to demonstrate EF Core logging strategies, covering Query Tags and Log Scopes to maximize logging potential.

## Requirements

* Visual Studio 2019+
* .NET Core 3.1+

### SQL Server

The project will by default create a new DB named `TwitterSample` on `.` server.

You can optionally comment the code for Sql Server and use SQLite instead in **Startup.cs** file in `ConfigureServices` method.
