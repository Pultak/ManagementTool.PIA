FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ManagementTool/Server/ManagementTool.Server.csproj", "ManagementTool/Server/"]
COPY ["ManagementTool/Client/ManagementTool.Client.csproj", "ManagementTool/Client/"]
COPY ["ManagementTool/Shared/ManagementTool.Shared.csproj", "ManagementTool/Shared/"]

RUN dotnet restore ManagementTool/Shared/ManagementTool.Shared.csproj
RUN dotnet restore ManagementTool/Client/ManagementTool.Client.csproj
RUN dotnet restore ManagementTool/Server/ManagementTool.Server.csproj
COPY . .


WORKDIR /src/ManagementTool/Shared
RUN dotnet build -c Release -o /app/build

WORKDIR /src/ManagementTool/Client
RUN dotnet build -c Release -o /app/build

WORKDIR /src/ManagementTool/Server
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ManagementTool.Server.csproj -c Release -o /app/publish
#/p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManagementTool.Server.dll"]
