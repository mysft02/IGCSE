# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution context
COPY ./BusinessObject ./BusinessObject
COPY ./Common ./Common
COPY ./Repository ./Repository
COPY ./Service ./Service
COPY ./Migration ./Migration
COPY ./IGCSE ./IGCSE

# Restore
RUN dotnet restore ./IGCSE/IGCSE.csproj

# Build
RUN dotnet publish ./IGCSE/IGCSE.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy built app
COPY --from=build /app/publish .

# Listen on the same HTTP port as launchSettings.json (http 5121)
ENV ASPNETCORE_URLS=http://+:5121
EXPOSE 5121

ENTRYPOINT ["dotnet", "IGCSE.dll"]


