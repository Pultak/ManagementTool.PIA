FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ManagementTool.sln ./
COPY ManagementTool/Client/ManagementTool.Client.csproj ./ManagementTool/Client/
COPY ManagementTool/Shared/ManagementTool.Shared.csproj ./ManagementTool/Shared/

RUN dotnet restore ManagementTool/Shared/ManagementTool.Shared.csproj
RUN dotnet restore ManagementTool/Client/ManagementTool.Client.csproj
COPY . .

WORKDIR /src/ManagementTool/Shared
RUN dotnet build -c Release -o /app

WORKDIR /src/ManagementTool/Client
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM nginx:alpine AS final
WORKDIR /app
COPY --from=publish /app/wwwroot .
COPY ManagementTool/Client/nginx.conf /etc/nginx/nginx.conf
