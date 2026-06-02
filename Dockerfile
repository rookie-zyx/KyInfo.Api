# =====================================================
# KyInfo API Dockerfile
# 多阶段构建，减小镜像体积
# =====================================================

# 第一阶段：构建应用
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["nuget.config", "./"]

# 复制项目文件
COPY ["KyInfo.Api/KyInfo.Api.csproj", "KyInfo.Api/"]
COPY ["src/KyInfo.Application/KyInfo.Application.csproj", "src/KyInfo.Application/"]
COPY ["src/KyInfo.Domain/KyInfo.Domain.csproj", "src/KyInfo.Domain/"]
COPY ["src/KyInfo.Contracts/KyInfo.Contracts.csproj", "src/KyInfo.Contracts/"]
COPY ["src/KyInfo.Infrastructure/KyInfo.Infrastructure.csproj", "src/KyInfo.Infrastructure/"]

# 还原依赖
RUN dotnet restore "KyInfo.Api/KyInfo.Api.csproj" --configfile nuget.config /p:NuGetAudit=false

# 复制所有源代码
COPY . .
WORKDIR "/src/KyInfo.Api"

# 发布应用
RUN dotnet publish "KyInfo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 第二阶段：运行应用
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 设置时区
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# 复制发布的应用
COPY --from=build /app/publish .

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# 启动应用
ENTRYPOINT ["dotnet", "KyInfo.Api.dll"]
