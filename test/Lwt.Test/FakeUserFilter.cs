namespace Lwt.Test
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Lwt.Models;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// fake user filter for testing.
    /// </summary>
    public class FakeUserFilter : IAsyncActionFilter
    {
        private readonly User user;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeUserFilter"/> class.
        /// </summary>
        public FakeUserFilter()
        {
            this.user = new User { Id = 1, UserName = "yolo", Email = "yolo@yolo.com" };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeUserFilter"/> class.
        /// </summary>
        /// <param name="user">the fake user.</param>
        public FakeUserFilter(User user)
        {
            this.user = user;
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claimIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("id", this.user.Id.ToString()),
                    new Claim("username", this.user.UserName),
                },
                "Basic");
            context.HttpContext.User.AddIdentity(claimIdentity);
            context.HttpContext.User = new ClaimsPrincipal(claimIdentity);
            await next();
        }
    }
}