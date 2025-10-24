# Use the official .NET 9.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["StudentPeformanceTracker.csproj", "."]
RUN dotnet restore "StudentPeformanceTracker.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "StudentPeformanceTracker.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "StudentPeformanceTracker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for Cloud Run
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "StudentPeformanceTracker.dll"]
