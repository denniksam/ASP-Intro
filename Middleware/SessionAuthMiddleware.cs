using Intro.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Intro.Middleware
{
    public class SessionAuthMiddleware
    {
        // обязательное поле (для Middleware)
        private readonly RequestDelegate next;  // ссылка на следующий слой 

        // обязательная форма конструктора
        public SessionAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        // обязательный метод класса
        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            String userId = context.Session.GetString("userId");
            if (userId != null)
            {
                authService.Set(userId);
            }
            
            context.Items.Add("fromAuthMiddleware", "Hello !!");
            await next(context);
        }
    }
}
