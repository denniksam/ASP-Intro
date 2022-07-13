using Intro.Models;
using Intro.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Intro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RandomService _randomService;
        private readonly IHasher _hasher;
        private readonly DAL.Context.IntroContext _introContext;
        private readonly IAuthService _authService;

        public HomeController(              // Внедрение зависимостей
            ILogger<HomeController> logger, // через конструктор
            RandomService randomService,
            IHasher hasher,
            DAL.Context.IntroContext introContext,
            IAuthService authService)  
        {
            _logger = logger;
            _randomService = randomService;
            _hasher = hasher;
            _introContext = introContext;
            _authService = authService;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            String userId = HttpContext.Session.GetString("userId");
            if (userId != null)
            {   // Была авторизация и в сессии хранится id пользователя

                // Извлекаем метку времени начала авторизации и вычисляем длительность
                long authMoment = Convert.ToInt64(
                    HttpContext.Session.GetString("AuthMoment"));

                // 10 000 000 тиков в секунде
                long authInterval = (DateTime.Now.Ticks - authMoment) / (long)1e7;

                if(authInterval > 10)  // Предельная длительность сеанса авторизации
                {
                    // Стираем из сессии признак авторизации
                    HttpContext.Session.Remove("userId");
                    HttpContext.Session.Remove("AuthMoment");
                    // По правилам безопасности: если меняется состояние авторизации
                    //  то необходимо перезагрузить систему (страницу)
                    HttpContext.Response.Redirect("/");
                    return;
                }

                ViewData["AuthUser"] = _introContext.Users.Find(Guid.Parse(userId));
            }
            base.OnActionExecuting(context);
        }

        public IActionResult Index()
        {
            ViewData["rnd"] = "<b>" + _randomService.Integer + "</b>";
            ViewBag.hash = _hasher.Hash("123");
            ViewData["UsersCount"] = _introContext.Users.Count();
            // Значение в HttpContext.Items добавлено в классе SessionAuthMiddleware
            ViewData["fromAuthMiddleware"] = HttpContext.Items["fromAuthMiddleware"];
            // проверяем службу авторизации
            ViewData["authUserName"] = _authService.User?.RealName;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            var model = new AboutModel
            {
                Data = "The Model Data"
            };
            return View(model);
        }

       
        public async Task<IActionResult> Random()
        {
            return Content(
                _randomService.Integer.ToString());
        }

        public IActionResult Data()
        {
            return Json(new { field = "value" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
                });
        }
    }
}
/*
 * Д.З. Вывести на стартовой странице всех
 * зарегистрированных в БД пользователей (только RealName)
 */