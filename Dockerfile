# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (leverages layer caching: copy csproj/sln first)
COPY *.sln ./
COPY Zust/Zust.Web.csproj Zust/
COPY Zust.Business/Zust.Business.csproj Zust.Business/
COPY Zust.Core/Zust.Core.csproj Zust.Core/
COPY Zust.DataAccess/Zust.DataAccess.csproj Zust.DataAccess/
COPY Zust.Entities/Zust.Entities.csproj Zust.Entities/
RUN dotnet restore Zust/Zust.Web.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish Zust/Zust.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# Render injects PORT; the app reads it in Program.cs and binds Kestrel accordingly.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Zust.Web.dll"]
