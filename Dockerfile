FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY smartPark/*.csproj ./smartPark/
WORKDIR /src/smartPark
# instalira dependecije
RUN dotnet restore

WORKDIR /src
COPY smartPark/. ./smartPark/

WORKDIR /src/smartPark
RUN dotnet publish -c Release -o /app/publish -r linux-x64 --self-contained false

# pokretanje aplikacije
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# curl za health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# non-root user
RUN useradd -m -s /bin/bash appuser && \
  chown -R appuser:appuser /app
USER appuser

EXPOSE 9090
ENV ASPNETCORE_URLS=http://+:9090

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "smartPark.dll"]
