using System;
using System.Net;
using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Models;
using Microsoft.AspNetCore.Http;

namespace Lwt.Middleware
{
    public class ExceptionHandleMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (BadRequestException exception)
            {
                await HandleBadRequestException(context, exception);
            }
            catch (ForbiddenException forbiddenException)
            {
                await HandleForbiddenException(context);
            }
            catch (NotFoundException notFoundException)
            {
                await HandleNotFoundException(context);
            }

            catch (Exception)
            {
                await HandleExceptionAsync(context);
            }
        }

        private Task HandleBadRequestException(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

            return context.Response.WriteAsync(new ErrorDetails
            (
                context.Response.StatusCode,
                exception.Message
            ).ToString());
        }

        private static Task HandleForbiddenException(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.Forbidden;

            return context.Response.WriteAsync(new ErrorDetails
            (
                context.Response.StatusCode,
                "Internal Server Error."
            ).ToString());
        }

        private static Task HandleNotFoundException(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.NotFound;

            return context.Response.WriteAsync(new ErrorDetails
            (
                context.Response.StatusCode,
                "Internal Server Error."
            ).ToString());
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new ErrorDetails
            (
                context.Response.StatusCode,
                "Internal Server Error."
            ).ToString());
        }
    }
}