#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and build files
COPY ["src/Directory.Build.props", "."]
COPY ["src/Directory.Build.targets", "."]
COPY ["src/Poxiao.sln", "."]
COPY ["src/global.json", "."]
COPY ["src/stylecop.json", "."]
COPY ["src/dotnet.ruleset", "."]
COPY ["src/.editorconfig", "."]

# Copy framework projects
COPY ["framework/Poxiao/Poxiao.csproj", "framework/Poxiao/"]
COPY ["framework/Poxiao.Extras.DatabaseAccessor.SqlSugar/Poxiao.Extras.DatabaseAccessor.SqlSugar.csproj", "framework/Poxiao.Extras.DatabaseAccessor.SqlSugar/"]
COPY ["framework/Poxiao.Extras.Authentication.JwtBearer/Poxiao.Extras.Authentication.JwtBearer.csproj", "framework/Poxiao.Extras.Authentication.JwtBearer/"]
COPY ["framework/Poxiao.Extras.Logging.Serilog/Poxiao.Extras.Logging.Serilog.csproj", "framework/Poxiao.Extras.Logging.Serilog/"]
COPY ["framework/Poxiao.Extras.ObjectMapper.Mapster/Poxiao.Extras.ObjectMapper.Mapster.csproj", "framework/Poxiao.Extras.ObjectMapper.Mapster/"]
COPY ["framework/Poxiao.Extras.DependencyModel.CodeAnalysis/Poxiao.Extras.DependencyModel.CodeAnalysis.csproj", "framework/Poxiao.Extras.DependencyModel.CodeAnalysis/"]

# Copy application project
COPY ["src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj", "src/application/Poxiao.API.Entry/"]

# Copy modularity projects (only the ones referenced)
COPY ["src/modularity/ai/Poxiao.AI/Poxiao.AI.csproj", "src/modularity/ai/Poxiao.AI/"]
COPY ["src/modularity/ai/Poxiao.AI.Web.Core/Poxiao.AI.Web.Core.csproj", "src/modularity/ai/Poxiao.AI.Web.Core/"]
COPY ["src/modularity/app/Poxiao.Apps/Poxiao.Apps.csproj", "src/modularity/app/Poxiao.Apps/"]
COPY ["src/modularity/codegen/Poxiao.CodeGen/Poxiao.CodeGen.csproj", "src/modularity/codegen/Poxiao.CodeGen/"]
COPY ["src/modularity/extend/Poxiao.Extend/Poxiao.Extend.csproj", "src/modularity/extend/Poxiao.Extend/"]
COPY ["src/modularity/kpi/Poxiao.Kpi.Web.Core/Poxiao.Kpi.Web.Core.csproj", "src/modularity/kpi/Poxiao.Kpi.Web.Core/"]
COPY ["src/modularity/lab/Poxiao.Lab/Poxiao.Lab.csproj", "src/modularity/lab/Poxiao.Lab/"]
COPY ["src/modularity/message/Poxiao.Message/Poxiao.Message.csproj", "src/modularity/message/Poxiao.Message/"]
COPY ["src/modularity/oauth/Poxiao.OAuth/Poxiao.OAuth.csproj", "src/modularity/oauth/Poxiao.OAuth/"]
COPY ["src/modularity/subdev/Poxiao.SubDev/Poxiao.SubDev.csproj", "src/modularity/subdev/Poxiao.SubDev/"]
COPY ["src/modularity/system/Poxiao.Systems/Poxiao.Systems.csproj", "src/modularity/system/Poxiao.Systems/"]
COPY ["src/modularity/taskschedule/Poxiao.TaskScheduler/Poxiao.TaskScheduler.csproj", "src/modularity/taskschedule/Poxiao.TaskScheduler/"]
COPY ["src/modularity/visualdata/Poxiao.VisualData/Poxiao.VisualData.csproj", "src/modularity/visualdata/Poxiao.VisualData/"]
COPY ["src/modularity/visualdev/Poxiao.VisualDev/Poxiao.VisualDev.csproj", "src/modularity/visualdev/Poxiao.VisualDev/"]
COPY ["src/modularity/workflow/Poxiao.WorkFlow/Poxiao.WorkFlow.csproj", "src/modularity/workflow/Poxiao.WorkFlow/"]

# Copy common projects
COPY ["src/modularity/common/Poxiao.Common.Core/Poxiao.Common.Core.csproj", "src/modularity/common/Poxiao.Common.Core/"]
COPY ["src/modularity/common/Poxiao.Common/Poxiao.Infrastructure.csproj", "src/modularity/common/Poxiao.Common/"]
COPY ["src/modularity/common/Poxiao.Common.CodeGen/Poxiao.Common.CodeGen.csproj", "src/modularity/common/Poxiao.Common.CodeGen/"]

# Restore packages
RUN dotnet restore "src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/application/Poxiao.API.Entry"
RUN dotnet build "Poxiao.API.Entry.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src"
RUN dotnet publish "src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
# Install curl for health check
RUN apt-get update && apt-get install -y curl
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Poxiao.API.Entry.dll"]