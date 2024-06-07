ARG DOTNET_SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0.203-bookworm-slim
ARG DOTNET_RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:8.0.3-bookworm-slim

FROM $DOTNET_RUNTIME_IMAGE AS base

# Install curl
RUN apt-get update && apt-get install -y curl

WORKDIR /app
EXPOSE 80

FROM $DOTNET_SDK_IMAGE AS build

ARG VERSION=1.0.0
ARG INFO_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["src/Hosts/Beter.Feed.TestingSandbox.Generator/Beter.Feed.TestingSandbox.Generator.csproj", "src/Hosts/Beter.Feed.TestingSandbox.Generator/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Hosting/Beter.Feed.TestingSandbox.Hosting.csproj", "src/Common/Beter.Feed.TestingSandbox.Hosting/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Logging.WebApi/Beter.Feed.TestingSandbox.Logging.WebApi.csproj", "src/Common/Beter.Feed.TestingSandbox.Logging.WebApi/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Logging/Beter.Feed.TestingSandbox.Logging.csproj", "src/Common/Beter.Feed.TestingSandbox.Logging/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Models/Beter.Feed.TestingSandbox.Models.csproj", "src/Common/Beter.Feed.TestingSandbox.Models/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Common/Beter.Feed.TestingSandbox.Common.csproj", "src/Common/Beter.Feed.TestingSandbox.Common/"]
RUN dotnet restore "./src/Hosts/Beter.Feed.TestingSandbox.Generator/./Beter.Feed.TestingSandbox.Generator.csproj"
COPY . .
WORKDIR "/src/src/Hosts/Beter.Feed.TestingSandbox.Generator"
RUN dotnet build "./Beter.Feed.TestingSandbox.Generator.csproj" -c $BUILD_CONFIGURATION  --no-restore

FROM build AS publish

ARG VERSION=1.0.0
ARG INFO_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Beter.Feed.TestingSandbox.Generator.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-build /p:UseAppHost=false /p:Version=$VERSION 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Beter.Feed.TestingSandbox.Generator.dll"]