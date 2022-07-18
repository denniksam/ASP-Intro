using Intro.DAL.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intro.API
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IntroContext _context;
        private readonly Services.IHasher _hasher;

        public UserController(IntroContext context, Services.IHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [HttpGet]
        public string Get(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: login required";
            }
            if (string.IsNullOrEmpty(password))
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: password required";
            }
            DAL.Entities.User user = 
                _context
                .Users
                .Where(u => u.Login == login)
                .FirstOrDefault();

            if (user == null)
            {
                HttpContext.Response.StatusCode = 401;
                return "Unauthorized: credentials rejected";
            }
            // Скопировано из AuthController - Login
            // 2. Хешируем соль + введенный пароль
            String PassHash = _hasher.Hash(password + user.PassSalt);

            // 3. Проверяем равенство полученного и хранимого хешей
            if (PassHash != user.PassHash)
            {
                HttpContext.Response.StatusCode = 401;
                return "Unauthorized: credentials invalid";
            }

            return user.Id.ToString();
        }

        // GET /api/user/0ab58465-6253-47a9-d48f-08da5b8a155b
        [HttpGet("{id}")]
        public object Get(String id)
        {
            Guid guid;
            // Validation
            try
            {
                guid = Guid.Parse(id);
            }
            catch
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: invalid id format (GUID required)";
            }
            // find user
            // return ( _context.Users.Find(guid) ?? new DAL.Entities.User() ) with { PassHash = "*", PassSalt = "*"};
            var user = _context.Users.Find(guid);
            if (user != null) return user with { PassHash = "*", PassSalt = "*" };
            return "null";
        }

        // POST api/<UserController>
        [HttpPost]
        public string Post([FromBody] string value)
        {
            return $"POST {value}";
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public object Put(String id, [FromForm] Models.RegUserModel userData)
        {
            return new { id, userData };
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
/* Д.З. Реализовать в методе PUT :
   - проверку id на GUID (если не соответствует, возвращать 409)
   - проверку на то, что существует пользователь с таким id (если нет, 404)
   - поэтапно проверить какие из полей переданы в качестве изменений и 
      подставить их в найденного пользователя, вернуть объект этого пользователя
      в JSON виде (в БД можно не сохранять)
 */