ARG DOTNET_SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:7.0
ARG DOTNET_RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:7.0.14-bullseye-slim

FROM $DOTNET_RUNTIME_IMAGE AS base
WORKDIR /app
EXPOSE 80

FROM $DOTNET_SDK_IMAGE AS build

ARG VERSION=1.0.0
ARG INFO_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["src/Hosts/Beter.TestingTools.Generator/Beter.TestingTools.Generator.csproj", "src/Hosts/Beter.TestingTools.Generator/"]
COPY ["src/Common/Beter.TestingTools.Hosting/Beter.TestingTools.Hosting.csproj", "src/Common/Beter.TestingTools.Hosting/"]
COPY ["src/Common/Beter.TestingTools.Logging.WebApi/Beter.TestingTools.Logging.WebApi.csproj", "src/Common/Beter.TestingTools.Logging.WebApi/"]
COPY ["src/Common/Beter.TestingTools.Logging/Beter.TestingTools.Logging.csproj", "src/Common/Beter.TestingTools.Logging/"]
COPY ["src/Common/Beter.TestingTools.Models/Beter.TestingTools.Models.csproj", "src/Common/Beter.TestingTools.Models/"]
COPY ["src/Common/Beter.TestingTools.Common/Beter.TestingTools.Common.csproj", "src/Common/Beter.TestingTools.Common/"]
RUN dotnet restore "./src/Hosts/Beter.TestingTools.Generator/./Beter.TestingTools.Generator.csproj"
COPY . .
WORKDIR "/src/src/Hosts/Beter.TestingTools.Generator"
RUN dotnet build "./Beter.TestingTools.Generator.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Version=$VERSION /p:AssemblyInformationalVersion=$INFO_VERSION

FROM build AS publish

ARG VERSION=1.0.0
ARG INFO_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Beter.TestingTools.Generator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=$VERSION 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Beter.TestingTools.Generator.dll"]