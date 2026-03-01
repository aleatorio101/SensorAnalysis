FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SensorAnalysis.API/ SensorAnalysis.API/
RUN dotnet restore SensorAnalysis.API/SensorAnalysis.API.csproj

RUN dotnet publish SensorAnalysis.API/SensorAnalysis.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SensorAnalysis.API.dll"]