# Dockerfile
ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build

WORKDIR /src
COPY ./AuthServer ./AuthServer

WORKDIR /src/AuthServer
RUN dotnet restore "./AuthServer.csproj"
RUN dotnet build "./AuthServer.csproj" -c "$BUILD_CONFIGURATION" -o /app/build
RUN dotnet publish "./AuthServer.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish -r linux-musl-x64

FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS run
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "AuthServer.dll" ]