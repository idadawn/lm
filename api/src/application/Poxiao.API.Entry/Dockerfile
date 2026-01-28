#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["application/Poxiao.API.Entry/Poxiao.API.Entry.csproj", "Poxiao.API.Entry/"]
RUN dotnet restore "Poxiao.API.Entry/Poxiao.API.Entry.csproj"
COPY . .
WORKDIR "/src/Poxiao.API.Entry"
RUN dotnet build "Poxiao.API.Entry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Poxiao.API.Entry.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
# Install curl for health check
RUN apt-get update && apt-get install -y curl
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Poxiao.API.Entry.dll"]