using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
using Site.Services.DataBaseServices.UsersService;
using Site.Services.EmailService;
using Site.Sources;

namespace Site.Pages.User
{
    public class ChangePasswordModel : PageModel
    {
        public ChangePasswordModel(IUserService dataBaseService)
        {
            dbService = dataBaseService;
        }

        readonly IUserService dbService;
        [BindProperty]
        public Model Value { get; set; }

        public ActionResult OnGet()
        {
            Value = new Model();
            string cookieEmail = Request.Cookies["Email"];

            if (cookieEmail != null)
            {
                Value.Email = cookieEmail;
            }
            else return RedirectToAction("/user/login");

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            // проверка валидации
            if (!ModelState.IsValid) return Page();

            Value.Email = Cleaner.СlearSpaces(Value.Email);   // удаление пробелов

            try
            {
                // ищем есть ли пользователь с такой электронной почтой?
                Users dbUser = dbService.Users.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (dbUser == null)
                {
                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Ошибка восстановления аккаунта.",
                        Text = "Пользователь с таким адресом электронной почты не зарегистрирован.",
                        RedirectPage = "/user/login"
                    });
                }

                // проверяем пароль
                CalculateHashString calculateHash = new();
                if (dbUser.PasswordHash != calculateHash.Calculate(Value.CurrentPassword))
                {
                    return RedirectToPage("/Info", new InfoModel.Model()
                    {
                        Header = "Ошибка изменения пароля.",
                        Text = "Текущий пароль введен неверно.",
                        RedirectPage = "/user/changepassword"
                    });
                }

                // вычисляем новый хеш нового пароля
                dbUser.PasswordHash = calculateHash.Calculate(Value.NewPassword);

                await dbService.SaveAsync();

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "Пароль изменен.",
                    Text = "",
                    RedirectPage = "/main"
                });
            }
            catch
            {
                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "Сервис временно недоступен.",
                    Text = "Просим извинения за временные неудобства.",
                    RedirectPage = "/main"
                });
            }
        }

        public class Model
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string NewPassword { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string CurrentPassword { get; set; }
        }
    }
}
