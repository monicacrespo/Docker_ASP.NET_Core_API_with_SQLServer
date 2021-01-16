# Entity Framework Core Containerized App with SQL Server 2017 within a Docker container in Visual Studio Code

The motivation is to configure ASP.NET Core to run on Docker, then to configure SQL Server on Docker. 

DockerGigApi contains the connection information to the database, and the SQL Server Container. 
And it is linked to the SQL Server container as shown in the image below.
Additionally, when the service startup, the database it is created and populated with an initial set of data.



![DatabaseInContainer](https://github.com/monicacrespo/Docker_ASP.NET_Core_API_with_SQLServer/blob/main/Images/DatabaseInContainer.JPG)


This is the connection string within the appsettings.json.
```
{
  "ConnectionStrings": {
    "SqlConnection": "Data Source=sqlserver;Database=GigDB;User Id=sa;Password=2Secure*Password2"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

Note: The hostname “sqlserver” does not exist in my machine or network.

And I have registered the database context in the startup class as below.
```
public void ConfigureServices(IServiceCollection services)
{
    var connection = Configuration.GetConnectionString("SqlConnection");
    IServiceCollection serviceCollections = services.AddDbContext<GigContext>(opts =>
    opts.UseSqlServer(connection,
	sqlServerOptionsAction: sqlOptions =>
	{
		sqlOptions.EnableRetryOnFailure();
	})
    );

    services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));
    services.AddScoped<IGigService, GigService>();
    services.AddControllers();
}
```		
		
This is the Dockerfile related to the DockerGigApi service. 
```
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DockerGigApi.csproj", "./"]
RUN dotnet restore "DockerGigApi.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DockerGigApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DockerGigApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerGigApi.dll"]
```

Since we want the ASP.NET Core container and the SQL Server container to run together, we need to create a Docker Compose project.
This is the docker-compose.yml. 
```
version: '3.4'

services:
  dockergigapi:
    links:  
      - sqlserver
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"

  sqlserver:  
    image: microsoft/mssql-server-linux:2017-latest
    hostname: 'sqlserver'
    environment:
      ACCEPT_EULA: Y  
      SA_PASSWORD: "2Secure*Password2" 
    ports:  
      - '1433:1433'
    expose:
      - 1433
```	
	

To feed the database with some data, I will use Entity Framework Core migrations, but I wouldn't use the migration in production environment. 

I added an initial migration to the project. This is done using the dotnet CLI:
dotnet ef migrations add InitialMigration

This is the Program.cs 
```
public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var gigContext = services.GetRequiredService<GigContext>();
                    await GigContextSeed.SeedAsync(gigContext, loggerFactory);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }
```

This is the GigContextSeed.cs
```
public async static Task SeedAsync(GigContext gigContext, ILoggerFactory loggerFactory, int? retry = 0)
        {
            var log = loggerFactory.CreateLogger<GigContextSeed>();
            
            int retryForAvailability = retry.Value;
            try
            {
                log.LogInformation("Applying migrations...");
                
                // TODO: Only run this if using a real database              
                gigContext.Database.Migrate();

                if (!gigContext.Gigs.Any())
                {
                    log.LogInformation("Adding data - seeding...");
                    gigContext.Gigs.AddRange(
                        GetPreconfiguredGigs());

                    await gigContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;    
                    log.LogError(ex.Message);
                    await SeedAsync(gigContext, loggerFactory, retryForAvailability);
                }
                throw;
            }
        }
```
Notes:
* Binds port 80 of the 'dockergigapi' Docker container to port 8080 of the host machine
* To access the api, we need to use port 8080 as follows: http://localhost:8080/api/gigs

# Dependencies
* Entity Framework Core Tools
```
dotnet tool install --global dotnet-ef --version 3.1.10
```

* Entity Framework Core SqlServer
```
dotnet add package Microsoft.EntityFrameworkCore.SqlServer -version 3.1.10
```
 

# Getting Started
To run the sample locally from Visual Studio Code:
* docker-compose up
* Press F5 and finally the browser displays the initial set of data http://localhost:5000/api/gigs
![Gigs](https://github.com/monicacrespo/Docker_ASP.NET_Core_API_with_SQLServer/blob/main/Images/GetGigs.JPG)
