# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GymAdmin.Api/GymAdmin.Api.csproj", "GymAdmin.Api/"]
COPY ["GymAdmin.Application/GymAdmin.Application.csproj", "GymAdmin.Application/"]
COPY ["GymAdmin.Domain/GymAdmin.Domain.csproj", "GymAdmin.Domain/"]
COPY ["GymAdmin.Infrastructure/GymAdmin.Infrastructure.csproj", "GymAdmin.Infrastructure/"]

RUN dotnet restore "GymAdmin.Api/GymAdmin.Api.csproj"

COPY . .

WORKDIR /src/GymAdmin.Api

RUN dotnet publish "GymAdmin.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libkrb5-3 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GymAdmin.Api.dll"]