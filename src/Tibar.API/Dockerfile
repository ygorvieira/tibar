FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /app

COPY . .
RUN dotnet restore src/Tibar.API/Tibar.API.csproj && \
    dotnet publish src/Tibar.API/Tibar.API.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

COPY --from=build /publish .
COPY src/Tibar.API/docker-entrypoint.sh .

RUN chmod +x docker-entrypoint.sh

ENV DB_HOST=db DB_PORT=5432

ENTRYPOINT ["./docker-entrypoint.sh"]
