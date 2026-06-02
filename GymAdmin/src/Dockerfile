# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["GymAdmin.Api/GymAdmin.Api.csproj", "GymAdmin.Api/"]
COPY ["GymAdmin.Application/GymAdmin.Application.csproj", "GymAdmin.Application/"]
COPY ["GymAdmin.Domain/GymAdmin.Domain.csproj", "GymAdmin.Domain/"]
COPY ["GymAdmin.Infrastructure/GymAdmin.Infrastructure.csproj", "GymAdmin.Infrastructure/"]
RUN dotnet restore "GymAdmin.Api/GymAdmin.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GymAdmin.Api"
RUN dotnet build "GymAdmin.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "GymAdmin.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GymAdmin.Api.dll"]
