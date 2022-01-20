using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
using Site.Services.DataBaseServices.UsersService;

namespace Site.Pages
{
    public class MainModel : PageModel
    {
        public MainModel(IConfiguration configuration, IUserService dataBaseService)
        {
            Configuration = configuration;
            this.dbService = dataBaseService;
        }

        public readonly IConfiguration Configuration;
        readonly IUserService dbService;

        public ActionResult OnGet()
        {
            string cookieEmail = Request.Cookies["Email"];
            string cookieToken = Request.Cookies["Token"];

            if (cookieEmail != null)
            {
                try
                {
                    // ищем есть ли пользователь с такой электронной почтой?
                    Users dbUser = dbService.Users.GetList().Where(p => p.Email == cookieEmail).FirstOrDefault();

                    CookieOptions cookieOptions = new();

                    if (dbUser == null)
                    {
                        // очищаем cookie
                        cookieOptions.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Append("Token", "", cookieOptions);
                        Response.Cookies.Append("Email", "", cookieOptions);

                        return RedirectToPage("/user/login");
                    }

                    // сравниваем токен
                    if (dbUser.Token != cookieToken)
                    {
                        // Обновляем почту в cookie и удаляем токен
                        cookieOptions.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Append("Email", cookieEmail, cookieOptions);
                        cookieOptions.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Append("Token", "", cookieOptions);

                        return RedirectToPage("/user/login");
                    }

                    // Обновляем cookie
                    cookieOptions.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Append("Email", cookieEmail, cookieOptions);
                    cookieOptions.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Append("Token", cookieToken, cookieOptions);

                    return Page();
                }
                catch
                {
                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Сервис временно недоступен.",
                        Text = "Просим извинения за временные неудобства.",
                        RedirectPage = "/user/login"
                    });
                }
            }
            else return RedirectToPage("/user/login");
        }
    }
}
