FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./backend/ ./
RUN dotnet restore "LibraryApp.sln"
RUN dotnet publish "API/API.csproj" -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y curl
ENTRYPOINT ["dotnet", "LibraryApp.API.dll"]
EXPOSE 8080