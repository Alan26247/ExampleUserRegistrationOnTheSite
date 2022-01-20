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
    public class CreateModel : PageModel
    {
        public CreateModel(IConfiguration configuration, IUserService dataBaseService,
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
                UsersRegistration dbRegistrationUser = dbService.UsersRegistration.GetList().Where(p => p.ActivationString == id).FirstOrDefault();
                if (dbRegistrationUser == null) return StatusCode(406);

                // проверяем не просрочился ли код
                if (DateTime.Now.Subtract(dbRegistrationUser.DateRegistration).TotalHours > 3)
                {
                    // просрочился удаляем его и возвращаем статус код
                    dbService.UsersRegistration.Delete(dbRegistrationUser.Id);
                    await dbService.SaveAsync();

                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Ошибка регистрации.",
                        Text = "Ссылка по которой вы перешли ошибочна либо просрочилась. Пройдите регистрацию заново..",
                        RedirectPage = "/user/login"
                    });
                }

                //----- добавляем пользователя
                TokenGenerator tokenGenerator = new();
                Users dbUser = new()
                {
                    Email = dbRegistrationUser.Email,
                    Login = dbRegistrationUser.Login,
                    DateTimeRegistration = DateTime.Now,
                    DateTimeLastVisit = DateTime.Now,
                    Token = tokenGenerator.Generate()
                };

                // вычисляем хеш пароля
                CalculateHashString calculateHash = new();
                dbUser.PasswordHash = calculateHash.Calculate(dbRegistrationUser.Password);

                // добавление в базу данных
                dbService.Users.Create(dbUser);

                // удаление регистрационной заметки из базы данных
                dbService.UsersRegistration.Delete(dbRegistrationUser.Id);
                await dbService.SaveAsync();

                // отправляем письмо
                emailService.SendEmailAsync(dbRegistrationUser.Email,
                    "Оповещение о регистрации аккаунта " + Configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>Оповещение о регистрации аккаунта на " + Configuration["Organization"] + "</h4>" +
                    $"<p>Ваш аккаунт успешно активирован на сервисе " + Configuration["Organization"] + ".</p>" +
                    $"<br />" +
                    $"<p>Данные для входа:</p>" +
                    $"<p>Электронная почта: " + dbRegistrationUser.Email + "</p>" +
                    $"<p>Пароль:            " + dbRegistrationUser.Password + "</p>" +
                    $"<br />" +
                    $"<p>Для входа на перейдите по ссылке: " + Configuration["ServerUrl"] + "/user/login </p>" +
                    $"<div/>");

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "Аккаунт активирован!",
                    Text = "Ваша электронная почта успешно подтверждена.",
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
