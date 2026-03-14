using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KyInfo.Api.OpenApi;

/// <summary>
/// 为 Swagger 补充统一错误响应结构（message）。
/// </summary>
public sealed class ErrorResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 仅补充常见错误响应，不强制覆盖已显式声明的响应。
        AddIfMissing(operation, "400", "参数错误");
        AddIfMissing(operation, "401", "未认证/无权限");
        AddIfMissing(operation, "403", "禁止访问");
        AddIfMissing(operation, "404", "资源不存在");
        AddIfMissing(operation, "500", "服务器错误");
    }

    private static void AddIfMissing(OpenApiOperation operation, string statusCode, string description)
    {
        if (operation.Responses.ContainsKey(statusCode))
        {
            return;
        }

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Required = new HashSet<string> { "message" },
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["message"] = new()
                            {
                                Type = "string",
                                Description = "错误信息"
                            }
                        }
                    }
                }
            }
        };
    }
}

