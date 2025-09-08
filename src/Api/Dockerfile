# ============== build stage ==============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia apenas os csproj para restaurar com cache eficiente
COPY ./src/Common/Common.csproj ./Common/
COPY ./src/Domain/Domain.csproj ./Domain/
COPY ./src/Application/Application.csproj ./Application/
COPY ./src/Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY ./src/Api/Api.csproj ./Api/

RUN dotnet restore ./Api/Api.csproj

# Copia o restante do código
COPY . .

# Publica somente a API
RUN dotnet publish ./src/Api/Api.csproj -c Release -o /app/publish

# ============== final stage ==============
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]
