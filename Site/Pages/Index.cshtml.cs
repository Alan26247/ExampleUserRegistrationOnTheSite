using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
using Site.Services.DataBaseServices.UsersService;
using System;
using System.Linq;

namespace Site.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IConfiguration configuration, IUserService dataBaseService)
        {
            Configuration = configuration;
            this.dbService = dataBaseService;
        }

        public readonly IConfiguration Configuration;
        readonly IUserService dbService;

        public ActionResult  OnGet()
        {
            string cookieEmail = Request.Cookies["Email"];
            string cookieToken = Request.Cookies["Token"];

            if (cookieEmail != null)
            {
                try
                {
                    // »щем есть ли пользователь с такой электронной почтой?
                    Users user = dbService.Users.GetList().Where(p => p.Email == cookieEmail).FirstOrDefault();

                    CookieOptions cookieOptions = new();
                    
                    if (user == null)
                    {
                        // очищаем cookie
                        cookieOptions.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Append("Token", "", cookieOptions);
                        Response.Cookies.Append("Email", "", cookieOptions);

                        return RedirectToPage("/user/login");
                    }

                    // сравниваем токен
                    if (user.Token != cookieToken)
                    {
                        // обновл€ем данные о почте в cookie и удал€ем токен 
                        cookieOptions.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Append("Email", cookieEmail, cookieOptions);
                        cookieOptions.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Append("Token", "", cookieOptions);

                        return RedirectToPage("/user/login");
                    }

                    // обновл€ем cookie
                    cookieOptions.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Append("Email", cookieEmail, cookieOptions);
                    cookieOptions.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Append("Token", cookieToken, cookieOptions);

                    return RedirectToPage("/main");
                }
                catch
                {
                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "—ервис временно недоступен.",
                        Text = "ѕросим извинени€ за временные неудобства.",
                        RedirectPage = "/user/login"
                    });
                }
            }

            return RedirectToPage("/user/login");
        }
    }
}
