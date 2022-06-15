using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JwtBearerExample.API.Server
{
	public class ActionFilterExample : ActionFilterAttribute
	{
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headers = context.HttpContext.Request.Headers;
            var request = context.ActionArguments.Values;

            return base.OnActionExecutionAsync(context, next);
        }
    }
}

