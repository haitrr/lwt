namespace Lwt
{
    using System;
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Newtonsoft.Json;

    /// <summary>
    /// etag filter.
    /// </summary>
    public class ETagFilterAttribute : Attribute, IActionFilter
    {
        private readonly int[] statusCodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ETagFilterAttribute"/> class.
        /// </summary>
        /// <param name="statusCodes">status codes.</param>
        public ETagFilterAttribute(params int[] statusCodes)
        {
            this.statusCodes = statusCodes;

            if (statusCodes.Length == 0)
            {
                this.statusCodes = new[] { 200 };
            }
        }

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.Method == "GET")
            {
                if (this.statusCodes.Contains(context.HttpContext.Response.StatusCode))
                {
                    string? content = JsonConvert.SerializeObject(context.Result);

                    var etag = ETagGenerator.GetETag(
                        context.HttpContext.Request.Path.ToString(),
                        Encoding.UTF8.GetBytes(content));

                    if (context.HttpContext.Request.Headers.Keys.Contains("If-None-Match") && context.HttpContext
                        .Request.Headers["If-None-Match"]
                        .ToString() == etag)
                    {
                        context.Result = new StatusCodeResult(304);
                    }

                    context.HttpContext.Response.Headers.Add("ETag", new[] { etag });
                }
            }
        }
    }
}