using System;
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
    public class RecoveryModel : PageModel
    {
        public RecoveryModel(IConfiguration configuration, IUserService dataBaseService,
           IEmailService emailService)
        {
            Configuration = configuration;
            this.dbService = dataBaseService;
            this.emailService = emailService;
        }

        public readonly IConfiguration Configuration;
        readonly IUserService dbService;
        readonly IEmailService emailService;

        public async Task<ActionResult> OnGet(string id)
        {
            if (id.Length > 256) return StatusCode(406);

            id = Cleaner.СlearWord(id);

            try
            {
                // проверяем есть ли такой активационный код
                UsersRecovery item = dbService.UsersRecovery.GetList().Where(p => p.ActivationString == id).FirstOrDefault();
                if (item == null) return StatusCode(406);

                // проверяем не просрочился ли код
                if (DateTime.Now.Subtract(item.DateRegistration).TotalHours > 3)
                {
                    // просрочился удаляем его и возвращаем статус код
                    dbService.UsersRegistration.Delete(item.Id);
                    await dbService.SaveAsync();

                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Ошибка восстановления аккаунта.",
                        Text = "Ссылка по которой вы перешли ошибочна либо просрочилась. Пройдите регистрацию восстановления заново..",
                        RedirectPage = "/user/login"
                    });
                }

                Users dbUser = dbService.Users.GetList().Where(p => p.Email == item.Email).FirstOrDefault();

                // создаем новый пароль
                PasswordGenerator passwordGenerator = new ();
                string newPassword = passwordGenerator.Generate(12);

                // вычисляем хеш пароля
                CalculateHashString calculateHash = new();
                dbUser.PasswordHash = calculateHash.Calculate(newPassword);

                await dbService.SaveAsync();

                // отправляем письмо
                emailService.SendEmailAsync(item.Email,
                    "Оповещение об восстановлении аккаунта " + Configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>Оповещение об восстановлении аккаунта на " + Configuration["Organization"] + "</h4>" +
                    $"<p>Ваш аккаунт успешно восстановлен! .</p>" +
                    $"<br />" +
                    $"<p>Данные для входа:</p>" +
                    $"<p>Электронная почта: " + item.Email + "</p>" +
                    $"<p>Пароль:            " + newPassword + "</p>" +
                    $"<br />" +
                    $"<p>Для входа на перейдите по ссылке: " + Configuration["ServerUrl"] + "/user/login </p>" +
                    $"<div/>");

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "Аккаунт успешно восстановлен!",
                    Text = "На вашу электронную почту было отправлено сообщение с новым паролем.",
                    RedirectPage = "/user/login"
                });
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
    }
}
