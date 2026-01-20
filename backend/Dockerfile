FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./backend/src ./
RUN dotnet restore "API/API.csproj"
RUN dotnet publish "API/API.csproj" -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y curl
ENTRYPOINT ["dotnet", "LibraryApp.API.dll"]
EXPOSE 8080