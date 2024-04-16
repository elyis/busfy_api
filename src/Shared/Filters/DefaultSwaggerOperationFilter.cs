using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace busfy_api.src.Shared.Filters
{
    public class DefaultSwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var roles = context.MethodInfo.
             GetCustomAttributes(true)
             .OfType<AuthorizeAttribute>()
             .Select(a => a.Roles)
             .Distinct()
             .ToArray();

            if (roles.Any())
                operation.Summary = $"Roles: {string.Join(", ", roles)}";
        }
    }
}