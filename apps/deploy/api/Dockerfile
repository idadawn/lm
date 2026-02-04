#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and build files
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["Poxiao.sln", "."]
COPY ["global.json", "."]
COPY ["stylecop.json", "."]
COPY ["dotnet.ruleset", "."]
COPY [".editorconfig", "."]

# Copy framework projects
COPY ["framework/Poxiao/Poxiao.csproj", "framework/Poxiao/"]
COPY ["framework/Poxiao.Extras.DatabaseAccessor.SqlSugar/Poxiao.Extras.DatabaseAccessor.SqlSugar.csproj", "framework/Poxiao.Extras.DatabaseAccessor.SqlSugar/"]
COPY ["framework/Poxiao.Extras.Authentication.JwtBearer/Poxiao.Extras.Authentication.JwtBearer.csproj", "framework/Poxiao.Extras.Authentication.JwtBearer/"]
COPY ["framework/Poxiao.Extras.Logging.Serilog/Poxiao.Extras.Logging.Serilog.csproj", "framework/Poxiao.Extras.Logging.Serilog/"]
COPY ["framework/Poxiao.Extras.ObjectMapper.Mapster/Poxiao.Extras.ObjectMapper.Mapster.csproj", "framework/Poxiao.Extras.ObjectMapper.Mapster/"]
COPY ["framework/Poxiao.Extras.DependencyModel.CodeAnalysis/Poxiao.Extras.DependencyModel.CodeAnalysis.csproj", "framework/Poxiao.Extras.DependencyModel.CodeAnalysis/"]

# Copy application project
COPY ["application/Poxiao.API.Entry/Poxiao.API.Entry.csproj", "application/Poxiao.API.Entry/"]

# Copy modularity projects (only the ones referenced)
COPY ["modularity/ai/Poxiao.AI/Poxiao.AI.csproj", "modularity/ai/Poxiao.AI/"]
COPY ["modularity/ai/Poxiao.AI.Web.Core/Poxiao.AI.Web.Core.csproj", "modularity/ai/Poxiao.AI.Web.Core/"]
COPY ["modularity/app/Poxiao.Apps/Poxiao.Apps.csproj", "modularity/app/Poxiao.Apps/"]
COPY ["modularity/codegen/Poxiao.CodeGen/Poxiao.CodeGen.csproj", "modularity/codegen/Poxiao.CodeGen/"]
COPY ["modularity/extend/Poxiao.Extend/Poxiao.Extend.csproj", "modularity/extend/Poxiao.Extend/"]
COPY ["modularity/kpi/Poxiao.Kpi.Web.Core/Poxiao.Kpi.Web.Core.csproj", "modularity/kpi/Poxiao.Kpi.Web.Core/"]
COPY ["modularity/lab/Poxiao.Lab/Poxiao.Lab.csproj", "modularity/lab/Poxiao.Lab/"]
COPY ["modularity/message/Poxiao.Message/Poxiao.Message.csproj", "modularity/message/Poxiao.Message/"]
COPY ["modularity/oauth/Poxiao.OAuth/Poxiao.OAuth.csproj", "modularity/oauth/Poxiao.OAuth/"]
COPY ["modularity/subdev/Poxiao.SubDev/Poxiao.SubDev.csproj", "modularity/subdev/Poxiao.SubDev/"]
COPY ["modularity/system/Poxiao.Systems/Poxiao.Systems.csproj", "modularity/system/Poxiao.Systems/"]
COPY ["modularity/taskschedule/Poxiao.TaskScheduler/Poxiao.TaskScheduler.csproj", "modularity/taskschedule/Poxiao.TaskScheduler/"]
COPY ["modularity/visualdata/Poxiao.VisualData/Poxiao.VisualData.csproj", "modularity/visualdata/Poxiao.VisualData/"]
COPY ["modularity/visualdev/Poxiao.VisualDev/Poxiao.VisualDev.csproj", "modularity/visualdev/Poxiao.VisualDev/"]
COPY ["modularity/workflow/Poxiao.WorkFlow/Poxiao.WorkFlow.csproj", "modularity/workflow/Poxiao.WorkFlow/"]

# Copy common projects
COPY ["modularity/common/Poxiao.Common.Core/Poxiao.Common.Core.csproj", "modularity/common/Poxiao.Common.Core/"]
COPY ["modularity/common/Poxiao.Common/Poxiao.Infrastructure.csproj", "modularity/common/Poxiao.Common/"]
COPY ["modularity/common/Poxiao.Common.CodeGen/Poxiao.Common.CodeGen.csproj", "modularity/common/Poxiao.Common.CodeGen/"]

# Restore packages
RUN dotnet restore "application/Poxiao.API.Entry/Poxiao.API.Entry.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/application/Poxiao.API.Entry"
RUN dotnet build "Poxiao.API.Entry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "application/Poxiao.API.Entry/Poxiao.API.Entry.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
# Install curl for health check
RUN apt-get update && apt-get install -y curl
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Poxiao.API.Entry.dll"]