ARG DOTNET_SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:7.0
ARG DOTNET_RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:7.0.14-bullseye-slim

FROM $DOTNET_RUNTIME_IMAGE AS base

# Install curl
RUN apt-get update && apt-get install -y curl

WORKDIR /app
EXPOSE 80

FROM --platform=$BUILDPLATFORM $DOTNET_SDK_IMAGE AS build

ARG VERSION=1.0.0
ARG INFO_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release
ARG TARGETPLATFORM
ARG TARGETOS
ARG TARGETARCH
ARG TARGETVARIANT
ARG BUILDPLATFORM
ARG BUILDOS
ARG BUILDARCH
ARG BUILDVARIANT

WORKDIR /src
COPY ["src/Hosts/Beter.Feed.TestingSandbox.Emulator/Beter.Feed.TestingSandbox.Emulator.csproj", "src/Hosts/Beter.Feed.TestingSandbox.Emulator/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Common/Beter.Feed.TestingSandbox.Common.csproj", "src/Common/Beter.Feed.TestingSandbox.Common/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Models/Beter.Feed.TestingSandbox.Models.csproj", "src/Common/Beter.Feed.TestingSandbox.Models/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Hosting/Beter.Feed.TestingSandbox.Hosting.csproj", "src/Common/Beter.Feed.TestingSandbox.Hosting/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Logging.WebApi/Beter.Feed.TestingSandbox.Logging.WebApi.csproj", "src/Common/Beter.Feed.TestingSandbox.Logging.WebApi/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Logging/Beter.Feed.TestingSandbox.Logging.csproj", "src/Common/Beter.Feed.TestingSandbox.Logging/"]
RUN set -eux; \
    echo "Building on \$BUILDPLATFORM=\"${BUILDPLATFORM}\", targeting \$TARGETPLATFORM=\"${TARGETPLATFORM}\""; \
    echo "Building on \$BUILDOS=\"${BUILDOS}\" and \$BUILDARCH=\"${BUILDARCH}\" with optional variant \$BUILDVARIANT=\"${BUILDVARIANT}\""; \
    echo "Targeting \$TARGETOS=\"${TARGETOS}\" and \$TARGETARCH=\"${TARGETARCH}\" with optional variant \$TARGETVARIANT=\"${TARGETVARIANT}\""; \
    dotnet restore -a $TARGETARCH "./src/Hosts/Beter.Feed.TestingSandbox.Emulator/Beter.Feed.TestingSandbox.Emulator.csproj";
COPY . .
WORKDIR "/src/src/Hosts/Beter.Feed.TestingSandbox.Emulator"
RUN dotnet build "./Beter.Feed.TestingSandbox.Emulator.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish

ARG VERSION=1.0.0
ARG INFO_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release
ARG TARGETPLATFORM
ARG TARGETOS
ARG TARGETARCH
ARG TARGETVARIANT
ARG BUILDPLATFORM
ARG BUILDOS
ARG BUILDARCH
ARG BUILDVARIANT

RUN set -eux; \
    echo "Building on \$BUILDPLATFORM=\"${BUILDPLATFORM}\", targeting \$TARGETPLATFORM=\"${TARGETPLATFORM}\""; \
    echo "Building on \$BUILDOS=\"${BUILDOS}\" and \$BUILDARCH=\"${BUILDARCH}\" with optional variant \$BUILDVARIANT=\"${BUILDVARIANT}\""; \
    echo "Targeting \$TARGETOS=\"${TARGETOS}\" and \$TARGETARCH=\"${TARGETARCH}\" with optional variant \$TARGETVARIANT=\"${TARGETVARIANT}\""; \
    dotnet publish "./Beter.Feed.TestingSandbox.Emulator.csproj" \
      -a $TARGETARCH \
      -c $BUILD_CONFIGURATION \
      -o /app/publish \
      /p:UseAppHost=false \
      /p:SelfContained=false \
      /p:Version=$VERSION;

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Beter.Feed.TestingSandbox.Emulator.dll"]