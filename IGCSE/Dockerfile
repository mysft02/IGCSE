FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file
COPY ["IGCSE/IGCSE.sln", "IGCSE/"]

# Copy all csproj files for dependency resolution (order: dependencies first)
COPY ["Common/Common.csproj", "Common/"]
COPY ["BusinessObject/BusinessObject.csproj", "BusinessObject/"]
COPY ["Repository/Repository.csproj", "Repository/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["Migration/Migration.csproj", "Migration/"]
COPY ["IGCSE/IGCSE.csproj", "IGCSE/"]

# Restore dependencies from solution file
WORKDIR "/src/IGCSE"
RUN dotnet restore "IGCSE.sln" --verbosity normal

# Copy everything else and build
WORKDIR "/src"
COPY . .
WORKDIR "/src/IGCSE"
# Build from solution to ensure all projects are built correctly
RUN dotnet build "IGCSE.sln" -c ${BUILD_CONFIGURATION} --no-restore && \
    dotnet build "IGCSE.csproj" -c ${BUILD_CONFIGURATION} -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/IGCSE"
# .NET SDK Web automatically includes wwwroot when publishing
RUN dotnet publish "IGCSE.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ensure wwwroot directory exists and has proper structure
RUN mkdir -p /app/wwwroot/avatar /app/wwwroot/certificates /app/wwwroot/courses/images /app/wwwroot/lessons/docs /app/wwwroot/lessons/videos || true

# Fix SkiaSharp Linux crash - Install all required dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libc6 \
    libgcc1 \
    libfreetype6 \
    libfontconfig1 \
    libharfbuzz0b \
    libharfbuzz-subset0 \
    libpng16-16 \
    libexpat1 \
    libicu-dev \
    libssl3 \
    libstdc++6 \
    zlib1g \
    && rm -rf /var/lib/apt/lists/*

# Install Java for Liquibase
RUN apt-get update && \
    apt-get install -y openjdk-17-jre-headless && \
    rm -rf /var/lib/apt/lists/*

# Set environment variable to listen on port 8081
ENV ASPNETCORE_URLS=http://+:8081

ENTRYPOINT ["dotnet", "IGCSE.dll"]

