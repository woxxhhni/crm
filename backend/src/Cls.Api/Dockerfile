# ========= Base (runtime) =========
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
# Expose HTTP port (we'll keep it simple, HTTP only in containers)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Forces Workstation Garbage Collection
ENV DOTNET_gcServer=0

# ========= Build (SDK) =========
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project files and restore dependencies to cache them
COPY ClsSolution.sln ./
COPY src/Cls.Shared/Cls.Shared.csproj src/Cls.Shared/
COPY src/Cls.Domain/Cls.Domain.csproj src/Cls.Domain/
COPY src/Cls.Application/Cls.Application.csproj src/Cls.Application/
COPY src/Cls.Infrastructure/Cls.Infrastructure.csproj src/Cls.Infrastructure/
COPY src/Cls.Api/Cls.Api.csproj src/Cls.Api/


# Restore packages (Api project pulls transitive refs; tests are excluded from the image)
RUN dotnet restore "src/Cls.Api/Cls.Api.csproj"

# Copy the remaining source code
COPY src/ ./src/


# Build and publish without restoring (since we just did)
RUN dotnet publish "src/Cls.Api/Cls.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore -p:DebugType=none

# ========= Final image =========
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Cls.Api.dll"]
