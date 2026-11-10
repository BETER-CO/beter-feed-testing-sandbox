# Build context: repository root.
#   docker buildx build -f src/Hosts/Beter.Feed.TestingSandbox.Server/Dockerfile .
# Set VERSION to stamp the assembly; leave it empty to keep the project's default version.

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

# Restore layer: only the files restore reads, so source edits keep the package cache.
COPY ["nuget.config", "Directory.Build.props", "Directory.Packages.props", "stylecop.json", ".editorconfig", ".globalconfig", "./"]
COPY ["src/Hosts/Beter.Feed.TestingSandbox.Server/Beter.Feed.TestingSandbox.Server.csproj", "src/Hosts/Beter.Feed.TestingSandbox.Server/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Common/Beter.Feed.TestingSandbox.Common.csproj", "src/Common/Beter.Feed.TestingSandbox.Common/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Models/Beter.Feed.TestingSandbox.Models.csproj", "src/Common/Beter.Feed.TestingSandbox.Models/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Hosting/Beter.Feed.TestingSandbox.Hosting.csproj", "src/Common/Beter.Feed.TestingSandbox.Hosting/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Logging/Beter.Feed.TestingSandbox.Logging.csproj", "src/Common/Beter.Feed.TestingSandbox.Logging/"]
COPY ["src/Common/Beter.Feed.TestingSandbox.Logging.WebApi/Beter.Feed.TestingSandbox.Logging.WebApi.csproj", "src/Common/Beter.Feed.TestingSandbox.Logging.WebApi/"]
RUN dotnet restore -a "$TARGETARCH" "src/Hosts/Beter.Feed.TestingSandbox.Server/Beter.Feed.TestingSandbox.Server.csproj"

COPY . .

# Publish restores again (a no-op when the layer above is complete), so a file
# the restore layer missed cannot produce a stale or broken image.
ARG VERSION=
RUN set -eu; \
    if [ -n "$VERSION" ]; then set -- "-p:Version=$VERSION"; else set --; fi; \
    dotnet publish "src/Hosts/Beter.Feed.TestingSandbox.Server/Beter.Feed.TestingSandbox.Server.csproj" \
      -a "$TARGETARCH" \
      -c Release \
      -o /app/publish \
      -p:UseAppHost=false \
      "$@"

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 51858
USER $APP_UID
ENTRYPOINT ["dotnet", "Beter.Feed.TestingSandbox.Server.dll"]
