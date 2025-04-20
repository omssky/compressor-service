# 1. Runtime base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5002

# 2. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build

COPY CompressorService.Api.csproj ./
RUN dotnet restore "CompressorService.Api.csproj"

COPY . ./
RUN dotnet build "CompressorService.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# 3. Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /build
RUN dotnet publish "CompressorService.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# 4. Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./

ENTRYPOINT ["dotnet", "CompressorService.Api.dll"]
