FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./Client/ ./
RUN dotnet restore "LibraryAppWeb.sln"
RUN dotnet publish "Web/Web.csproj" -c release -o /app --no-restore
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y curl
ENTRYPOINT ["dotnet", "LibraryAppWeb.Web.dll"]