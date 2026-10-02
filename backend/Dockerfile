# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so package downloads are cached until a project file changes.
COPY global.json Directory.Build.props AiOps.slnx ./
COPY src/AiOps.Domain/AiOps.Domain.csproj src/AiOps.Domain/
COPY src/AiOps.Application/AiOps.Application.csproj src/AiOps.Application/
COPY src/AiOps.Infrastructure/AiOps.Infrastructure.csproj src/AiOps.Infrastructure/
COPY src/AiOps.Api/AiOps.Api.csproj src/AiOps.Api/
RUN dotnet restore src/AiOps.Api/AiOps.Api.csproj

COPY src/ src/
RUN dotnet publish src/AiOps.Api/AiOps.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "AiOps.Api.dll"]
