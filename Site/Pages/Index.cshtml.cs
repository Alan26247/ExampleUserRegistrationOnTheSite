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
                    // ���� ���� �� ������������ � ����� ����������� ������?
                    Users user = dbService.Users.GetList().Where(p => p.Email == cookieEmail).FirstOrDefault();

                    CookieOptions cookieOptions = new();
                    
                    if (user == null)
                    {
                        // ������� cookie
                        cookieOptions.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Append("Token", "", cookieOptions);
                        Response.Cookies.Append("Email", "", cookieOptions);

                        return RedirectToPage("/user/login");
                    }

                    // ���������� �����
                    if (user.Token != cookieToken)
                    {
                        // ��������� ������ � ����� � cookie � ������� ����� 
                        cookieOptions.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Append("Email", cookieEmail, cookieOptions);
                        cookieOptions.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Append("Token", "", cookieOptions);

                        return RedirectToPage("/user/login");
                    }

                    // ��������� cookie
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
                        Header = "������ �������� ����������.",
                        Text = "������ ��������� �� ��������� ����������.",
                        RedirectPage = "/user/login"
                    });
                }
            }

            return RedirectToPage("/user/login");
        }
    }
}
