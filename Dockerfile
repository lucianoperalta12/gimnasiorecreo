# ========== Stage 1: Build Frontend ==========
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend

# Install dependencies
COPY gym-frontend/package*.json ./
RUN npm ci

# Build frontend
COPY gym-frontend/ .
RUN npm run build

# ========== Stage 2: Build Backend ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy project files and restore
COPY GymAdmin/src/GymAdmin.Api/GymAdmin.Api.csproj GymAdmin.Api/
COPY GymAdmin/src/GymAdmin.Application/GymAdmin.Application.csproj GymAdmin.Application/
COPY GymAdmin/src/GymAdmin.Domain/GymAdmin.Domain.csproj GymAdmin.Domain/
COPY GymAdmin/src/GymAdmin.Infrastructure/GymAdmin.Infrastructure.csproj GymAdmin.Infrastructure/
RUN dotnet restore "GymAdmin.Api/GymAdmin.Api.csproj"

# Build and publish backend
COPY GymAdmin/src/ .
WORKDIR /src/GymAdmin.Api
RUN dotnet publish "GymAdmin.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========== Stage 3: Final Runtime ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published backend
COPY --from=backend-build /app/publish .

# Copy built frontend to wwwroot of the backend
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GymAdmin.Api.dll"]
