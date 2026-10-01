# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY global.json nuget.config ./
COPY src/EcoCheck.Api/EcoCheck.Api.csproj src/EcoCheck.Api/
RUN dotnet restore src/EcoCheck.Api/EcoCheck.Api.csproj

COPY src/ src/
RUN dotnet publish src/EcoCheck.Api/EcoCheck.Api.csproj -c Release -o /app --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_NOLOGO=true

COPY --from=build /app ./

# Usuário sem privilégios, já existente na imagem oficial.
USER $APP_UID

# Porta padrão da imagem; no Railway a variável PORT tem prioridade (ver Program.cs).
EXPOSE 8080

ENTRYPOINT ["dotnet", "EcoCheck.Api.dll"]
