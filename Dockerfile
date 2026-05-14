# ========== Stage 1: Build Frontend ==========
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY gym-frontend/package*.json ./
RUN npm ci
COPY gym-frontend/ .
RUN npm run build

# ========== Stage 2: Build Backend ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Restore
COPY GymAdmin/src/GymAdmin.Api/GymAdmin.Api.csproj GymAdmin.Api/
COPY GymAdmin/src/GymAdmin.Application/GymAdmin.Application.csproj GymAdmin.Application/
COPY GymAdmin/src/GymAdmin.Domain/GymAdmin.Domain.csproj GymAdmin.Domain/
COPY GymAdmin/src/GymAdmin.Infrastructure/GymAdmin.Infrastructure.csproj GymAdmin.Infrastructure/
RUN dotnet restore "GymAdmin.Api/GymAdmin.Api.csproj"

# Build
COPY GymAdmin/src/ .
WORKDIR /src/GymAdmin.Api
RUN dotnet publish "GymAdmin.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========== Stage 3: Final Runtime ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/frontend/dist ./wwwroot

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "GymAdmin.Api.dll"]
