using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            ApiParameterDescription description = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(x => x.ParameterDescriptor.ParameterType == typeof(IFormFile));

            if (description != null)
            {
                IParameter parameter = operation.Parameters.First(o => o.Name == description.Name);
                operation.Parameters.Remove(parameter);

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = parameter.Name,
                    In = "formData",
                    Description = parameter.Description,
                    Required = parameter.Required,
                    Type = "file"
                });
                operation.Consumes.Add("application/form-data");
            }
        }
    }
}
