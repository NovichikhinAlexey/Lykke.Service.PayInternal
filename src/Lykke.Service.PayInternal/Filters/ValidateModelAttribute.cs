using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Extensions;

namespace Lykke.Service.PayInternal.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new ErrorResponse().AddErrors(context.ModelState));
            }
        }
    }
}
