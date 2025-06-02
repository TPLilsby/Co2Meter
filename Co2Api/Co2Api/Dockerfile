# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj og gendan afhængigheder
COPY *.csproj ./
RUN dotnet restore

# Copy resten af appen og build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy fra build stage
COPY --from=build /app/publish .

# Eksponér port 80
EXPOSE 80

# Kør appen
ENTRYPOINT ["dotnet", "Co2Api.dll"]
