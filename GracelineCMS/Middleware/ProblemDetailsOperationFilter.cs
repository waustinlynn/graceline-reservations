using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GracelineCMS.Middleware
{
    public class ProblemDetailsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Responses == null) return;

            // Use the actual ProblemDetails type instead of manually defining it
            var problemDetailsSchema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);

            // Common error status codes
            var errorStatuses = new[] { 400, 401, 403, 404, 500 };

            foreach (var statusCode in errorStatuses)
            {
                operation.Responses[statusCode.ToString()] = new OpenApiResponse
                {
                    Description = $"Error {statusCode}",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = problemDetailsSchema
                        }
                    }
                };
            }
        }
    }
}
