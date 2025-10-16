using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portfolio.WebAPI.Filter
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParams = context.ApiDescription.ParameterDescriptions
                .Where(p => p.ModelMetadata != null &&
                           (p.ModelMetadata.ModelType == typeof(IFormFile) ||
                            p.ModelMetadata.ModelType == typeof(List<IFormFile>) ||
                            p.ModelMetadata.ModelType == typeof(IEnumerable<IFormFile>) ||
                            p.ModelMetadata.ModelType == typeof(IFormFileCollection)))
                .ToList();

            if (!formFileParams.Any())
                return;

            // Check if action has [Consumes("multipart/form-data")]
            var consumesAttribute = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Mvc.ConsumesAttribute>()
                .FirstOrDefault();

            if (consumesAttribute?.ContentTypes.Contains("multipart/form-data") != true)
                return;

            // Clear parameters that will be in request body
            var parametersToRemove = operation.Parameters
                .Where(p => formFileParams.Any(fp => fp.Name == p.Name))
                .ToList();

            foreach (var param in parametersToRemove)
            {
                operation.Parameters.Remove(param);
            }

            // Create request body schema
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredParams = new HashSet<string>();

            foreach (var param in context.ApiDescription.ParameterDescriptions)
            {
                if (param.Source?.Id != "Form")
                    continue;

                var paramType = param.ModelMetadata?.ModelType;
                if (paramType == null)
                    continue;

                var paramName = param.Name;

                if (paramType == typeof(IFormFile))
                {
                    properties[paramName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else if (paramType == typeof(List<IFormFile>) ||
                         paramType == typeof(IEnumerable<IFormFile>) ||
                         paramType == typeof(IFormFileCollection))
                {
                    properties[paramName] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    };
                }
                else if (paramType == typeof(string))
                {
                    properties[paramName] = new OpenApiSchema { Type = "string" };
                }
                else if (paramType == typeof(int) || paramType == typeof(int?))
                {
                    properties[paramName] = new OpenApiSchema { Type = "integer", Format = "int32" };
                }
                else if (paramType == typeof(bool) || paramType == typeof(bool?))
                {
                    properties[paramName] = new OpenApiSchema { Type = "boolean" };
                }

                if (param.IsRequired)
                {
                    requiredParams.Add(paramName);
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = requiredParams
                        }
                    }
                }
            };
        }
    }
}