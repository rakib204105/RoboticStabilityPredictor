# Use Microsoft's official .NET image.
# https://hub.docker.com/_/microsoft-dotnet-sdk
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY ["RoboticStabilityPredictor/RoboticStabilityPredictor.csproj", "RoboticStabilityPredictor/"]
RUN dotnet restore "RoboticStabilityPredictor/RoboticStabilityPredictor.csproj"

# Copy everything else and build app
COPY . .
WORKDIR /source/RoboticStabilityPredictor
RUN dotnet publish -c Release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
 
# Expose port (Render sets PORT environment variable, defaulting to 8080 usually)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "RoboticStabilityPredictor.dll"]
