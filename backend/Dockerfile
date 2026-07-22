# ---- build stage: compile + publish with the .NET SDK ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first so `restore` is cached until dependencies change.
COPY src/GameHub.API/*.csproj src/GameHub.API/
COPY src/GameHub.Application/*.csproj src/GameHub.Application/
COPY src/GameHub.Domain/*.csproj src/GameHub.Domain/
COPY src/GameHub.Infrastructure/*.csproj src/GameHub.Infrastructure/
RUN dotnet restore src/GameHub.API/GameHub.API.csproj

# Then the source, and publish a trimmed, release build.
COPY src/ src/
RUN dotnet publish src/GameHub.API/GameHub.API.csproj -c Release -o /app --no-restore

# ---- runtime stage: slim ASP.NET image, no SDK ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
# The aspnet image listens on 8080 by default (ASPNETCORE_URLS=http://+:8080).
EXPOSE 8080
ENTRYPOINT ["dotnet", "GameHub.API.dll"]
