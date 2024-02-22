FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# remote debbug
RUN apt-get update && apt-get install wget -y
RUN mkdir -p ~/.vs-debugger; wget https://aka.ms/getvsdbgsh -O ~/.vs-debugger/GetVsDbg.sh; chmod a+x ~/.vs-debugger/GetVsDbg.sh

COPY ./Server/ ./
RUN dotnet restore "LibraryApp.sln"
RUN dotnet publish "API/API.csproj" -c release -o /app --no-restore
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y curl
ENTRYPOINT ["dotnet", "LibraryApp.API.dll"]