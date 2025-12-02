# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 4567
ENV ASPNETCORE_URLS=http://+:4567
ENV ASPNETCORE_ENVIRONMENT=Production

# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["identity-service/identity-service.csproj", "identity-service/"]
RUN dotnet restore "identity-service/identity-service.csproj"
COPY . .
WORKDIR "/src/identity-service"
RUN dotnet build "identity-service.csproj" -c $configuration -o /app/build 

# Publish stage
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "identity-service.csproj" -c $configuration -o /app/publish /p:UseAppHost=false 

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "identity-service.dll"]