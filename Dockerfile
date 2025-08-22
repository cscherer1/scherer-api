# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy the csproj and restore first (leverages Docker layer cache)
COPY ./src/Scherer.Api/Scherer.Api.csproj ./src/Scherer.Api/
RUN dotnet restore ./src/Scherer.Api/Scherer.Api.csproj

# copy everything and publish
COPY . .
RUN dotnet publish ./src/Scherer.Api/Scherer.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Scherer.Api.dll"]
