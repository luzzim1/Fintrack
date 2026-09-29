FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FinTrack.Domain/FinTrack.Domain.csproj FinTrack.Domain/
COPY FinTrack.Application/FinTrack.Application.csproj FinTrack.Application/
COPY FinTrack.Infrastructure/FinTrack.Infrastructure.csproj FinTrack.Infrastructure/
COPY FinTrack.Api/FinTrack.Api.csproj FinTrack.Api/
RUN dotnet restore FinTrack.Api/FinTrack.Api.csproj

COPY . .
RUN dotnet publish FinTrack.Api/FinTrack.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "FinTrack.Api.dll"]
