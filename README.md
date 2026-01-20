# Introduction 
This is a simple POC to understand the .NET ecosystem with C# as the main language, Entity Framework as ORM framework, and Blazor App as the Web app.

# Getting Started
To start building/running the system, it will be necessary to have installed: Docker, to run SQL Server, MongoDB, the server, and the client app; .NET SDK 7.0; as well as the best code editor of your choice. This project uses VS Code.

To start your own project just follow:

Server
1. Start the backend app with the API project:  `dotnet new webapi --name API`
2. Create the solution file:                    `dotnet new sln --name LibraryApp`
3. Add the project(s) to solution:              `dotnet sln add .\API\API.csproj`

Client
1. Create the frontend web app:                 `dotnet new blazorserver-empty --name Web`
2. Create the solution file:                    `dotnet new sln --name LibraryAppWeb`
3. Add web project to solution:                 `dotnet sln add .\Web\Web.csproj`

# Build and Test
In the root folder run: `docker compose up -d --build`

# Add (or remove last) new migrations in backend
`dotnet ef migrations add "migration name" --project src/Auth/Auth.Infrastructure --startup-project src/API --output-dir Migrations` <br>
`dotnet ef migrations remove --project src/Auth/Auth.Infrastructure --startup-project src/API` <br>
`dotnet ef database update --project src/Auth/Auth.Infrastructure --startup-project src/API`