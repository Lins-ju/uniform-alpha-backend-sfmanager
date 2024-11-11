# THIS DOCKERFILE IS MEANT TO BE BUILT WITH ANOTHER PATH. THE FOLLOWING COMMAND IS THE EXACT WAY TO DO IT.
# docker build -f ./SFManager/Dockerfile ./ -t sf-system-{versionNumber}

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SFManager/SFManager.csproj", "SFManager/"]
COPY ["Utils/Utils.csproj", "Utils/"]
RUN dotnet restore "SFManager/SFManager.csproj"

COPY . .
WORKDIR "/src/SFManager"
RUN dotnet build "SFManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SFManager.dll"]