using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
using Site.Services.DataBaseServices.UsersService;
using Site.Sources;

namespace Site.Pages.User
{
    public class LoginModel : PageModel
    {
        public LoginModel(IUserService dataBaseService)
        {
            dbService = dataBaseService;
        }

        readonly IUserService dbService;
        [BindProperty]
        public Model Value { get; set; }

        public void OnGet()
        {
            Value = new Model();

            CookieOptions cookieOptions = new();

            if (Request.Cookies["Email"] != null)
            {
                Value.Email = Request.Cookies["Email"];

                // Обновляем cookie
                cookieOptions.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Append("Email", Value.Email, cookieOptions);
                cookieOptions.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append("Token", "", cookieOptions);
            }
            else
            {
                // Обновляем cookie
                cookieOptions.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append("Token", "", cookieOptions);
                Response.Cookies.Append("Email", "", cookieOptions);
            }
        }

        public async Task<ActionResult> OnPost()
        {
            // проверка валидации
            if (!ModelState.IsValid) return Page();

            Value.Email = Cleaner.СlearSpaces(Value.Email);   // удаление пробелов

            CookieOptions cookieOptions = new();

            // Ищем есть ли пользователь с такой электронной почтой?
            Users dbUser = dbService.Users.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
            if (dbUser == null)
            {
                // Обновляем cookie
                cookieOptions.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append("Token", "", cookieOptions);
                Response.Cookies.Append("Email", "", cookieOptions);

                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "Ошибка авторизации.",
                    Text = "Пользователь с таким адресом электронной почты не зарегистрирован.",
                    RedirectPage = "/user/login"
                });
            }

            // проверяем пароль
            CalculateHashString calculateHash = new();
            if (dbUser.PasswordHash != calculateHash.Calculate(Value.Password))
            {
                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "Ошибка авторизации.",
                    Text = "Ошибка пароля",
                    RedirectPage = "/user/login"
                });
            }

            // создаем новый токен
            TokenGenerator tokenGenerator = new();
            dbUser.Token = tokenGenerator.Generate();
            dbUser.DateTimeLastVisit = DateTime.Now;
            await dbService.SaveAsync();

            // Обновляем cookie
            cookieOptions.Expires = DateTime.Now.AddDays(7);
            Response.Cookies.Append("Token", dbUser.Token, cookieOptions);
            Response.Cookies.Append("Email", dbUser.Email, cookieOptions);

            return RedirectToPage("/main");

        }

        public class Model
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string Password { get; set; }
        }
    }
}
