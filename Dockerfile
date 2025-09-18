# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-tools
RUN apt-get update -y
RUN apt-get install -y curl
RUN curl --silent --location https://deb.nodesource.com/setup_22.x | bash -
RUN apt-get install -y nodejs
RUN npm i -g @quasar/cli

# This stage is used to build the service project
FROM build-tools AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["StreamBuddy.Web/StreamBuddy.Web.csproj", "StreamBuddy.Web/"]
RUN dotnet restore "./StreamBuddy.Web/StreamBuddy.Web.csproj"
COPY . .
# build client
WORKDIR "/src/client"
RUN npm install
RUN quasar build
# build server
WORKDIR "/src/StreamBuddy.Web"
RUN dotnet build "./StreamBuddy.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StreamBuddy.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StreamBuddy.Web.dll"]