# KyInfo.Api

KyInfo 的 **ASP.NET Core Web API** 宿主项目：对外提供 REST 接口，集成 JWT 认证、FluentValidation、固定窗口限流、开发环境 Swagger，并注册 `KyInfo.Application` 与 `KyInfo.Infrastructure` 中的业务与数据访问能力。

## 与本仓库其它项目的关系

| 引用 | 作用 |
|------|------|
| [KyInfo.Application](../src/KyInfo.Application) | 应用服务与用例 |
| [KyInfo.Infrastructure](../src/KyInfo.Infrastructure) | EF Core、仓储、AI 网关等基础设施 |
| [KyInfo.Domain](../src/KyInfo.Domain) / [KyInfo.Contracts](../src/KyInfo.Contracts) | 领域与契约（经上述层间接使用） |

前端 [KyInfo.Blazor](../KyInfo.Blazor) 通过 HTTP 调用本 API；开发环境下 CORS 默认允许 Blazor 常用地址（见 `appsettings.Development.json` 中 `Cors:Origins`）。

## 主要能力（概览）

- **控制器**：认证与账号、院校与专业、招生信息、分数线、考试成绩、志愿推荐、AI 对话、管理端（如成绩导入）等（见 `Controllers/`）。
- **认证**：JWT Bearer；`MapInboundClaims = false`，角色 claim 使用短名称 `role`（与 `[Authorize(Roles = ...)]` 一致）。
- **校验**：`Validators/` 中 FluentValidation，与控制器请求模型配合。
- **限流**：登录、注册、AI 聊天、管理端上传等路由使用独立策略，超限返回 **429** 与 JSON 提示（详见仓库根目录 [docs/CONFIGURATION.md](../docs/CONFIGURATION.md)）。
- **OpenAPI**：Development 下启用 Swagger / Swagger UI；支持在 UI 中填写 Bearer Token。
- **种子数据**：启动时执行 `AdminSeeder`（仅当配置启用），选项见 `Seed/` 与配置节 `Seed:Admin`。
- **中间件**：`Middleware/ExceptionHandlingMiddleware` 统一异常响应。

## 目录结构（常用部分）

| 路径 | 说明 |
|------|------|
| `Program.cs` | 服务注册、管道、JWT/CORS/限流 |
| `Controllers/` | API 控制器 |
| `Validators/` | FluentValidation 验证器 |
| `Middleware/` | HTTP 中间件 |
| `OpenApi/` | Swagger 扩展（如错误响应说明） |
| `Seed/` | 管理员种子逻辑与选项 |
| `Migrations/` | EF Core 迁移 **源码位置**；本 csproj 通过 `Compile Remove` **不在本程序集编译**，由 Infrastructure 以链接方式编译（避免重复类型）。 |

## 运行

在**仓库根目录**（与 `KyInfo.sln` 同级）推荐：

```powershell
dotnet run --project KyInfo.Api\KyInfo.Api.csproj
```

或在 `KyInfo.Api` 目录下：

```powershell
dotnet run
```

默认 HTTPS 开发地址见 [Properties/launchSettings.json](Properties/launchSettings.json)（例如 `https://localhost:7233`）。Development 下 Swagger：`/swagger`。

## 配置说明

| 文件 | 说明 |
|------|------|
| [appsettings.json](appsettings.json) | 连接字符串、`Jwt`、`Ai`、`Logging`、`AllowedHosts` 等基线配置 |
| [appsettings.Development.json](appsettings.Development.json) | 开发日志、`Cors:Origins`、可选 `Seed:Admin` |
| `appsettings.Local.json`（可选，勿提交） | 由 `Program.cs` 可选加载，用于本地密钥或覆盖 |

**安全**：勿将生产 JWT 密钥、数据库密码、AI Key 写入 Git。本地可用 User Secrets（本项目的 `UserSecretsId` 见 [KyInfo.Api.csproj](KyInfo.Api.csproj)）或 `appsettings.Local.json`。

详细清单与生产注意事项见 [docs/CONFIGURATION.md](../docs/CONFIGURATION.md)；令牌行为见 [docs/SECURITY-TOKENS.md](../docs/SECURITY-TOKENS.md)。

## 数据库迁移

迁移 C# 文件位于本目录下的 `Migrations/`，但编译进 **Infrastructure** 程序集。在仓库根目录执行：

```powershell
dotnet ef database update --project src\KyInfo.Infrastructure\KyInfo.Infrastructure.csproj --startup-project KyInfo.Api\KyInfo.Api.csproj --context AppDbContext
```

（需已安装 [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) 全局工具。）

## 集成测试

测试项目通过 `WebApplicationFactory` 等方式引用本 API；当环境名为 `Testing` 时，管道会跳过部分与测试主机冲突的配置（如 `Program.cs` 中的 HTTPS 重定向）。详见 [tests/KyInfo.Tests](../tests/KyInfo.Tests)。

## 更多文档

- [仓库根 README](../README.md) — 整体快速开始与仓库结构
