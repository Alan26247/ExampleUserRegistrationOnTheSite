using System;
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
    public class RegistrationModel : PageModel
    {
        public RegistrationModel(IConfiguration configuration, IUserService dataBaseService,
                                                                    IEmailService emailService)
        {
            this.configuration = configuration;
            dbService = dataBaseService;
            this.emailService = emailService;
        }

        readonly IConfiguration configuration;
        readonly IUserService dbService;
        readonly IEmailService emailService;
        [BindProperty]
        public Model Value { get; set; }

        public async Task<ActionResult> OnPost()
        {
            // проверка валидации
            if (!ModelState.IsValid) return Page();

            Value.Email = Cleaner.СlearSpaces(Value.Email);   // удаление пробелов

            try
            {
                // Ищем есть ли пользователь уже с такой электронной почтой?
                Users dbUser = dbService.Users.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (dbUser != null)
                {
                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Ошибка регистрации.",
                        Text = "Пользователь с таким адресом электронной почты уже зарегистрирован.",
                        RedirectPage = "/user/login"
                    });
                } 
                    
                // проверяем не отправляли ли мы уже запрос на подтверждение данному пользователю
                UsersRegistration dbRegistrationUsers = dbService.UsersRegistration.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (dbRegistrationUsers != null)
                {
                    // проверяем не просрочился ли код
                    if (DateTime.Now.Subtract(dbRegistrationUsers.DateRegistration).TotalHours > 3)
                    {
                        // просрочился удаляем его и возвращаем статус код
                        dbService.UsersRegistration.Delete(dbRegistrationUsers.Id);
                    }
                    else
                    {
                        return RedirectToPage("/info", new InfoModel.Model()
                        {
                            Header = "Ошибка регистрации.",
                            Text = "Вы уже отправляли запрос на регистрацию ранее. На вашу электронную почту ранее уже отправлялось письмо. Для подтверждения регистрации  перейдите по ссылке в письме.",
                            RedirectPage = "/user/login"
                        });
                    }
                }

                // создание регистрации пользователя
                dbRegistrationUsers = new UsersRegistration
                {
                    Email = Value.Email,
                    Login = Value.Login,
                    DateRegistration = DateTime.Now,
                    Password = Value.Password
                };

                // здесь генерация нового кода и отправка его на email пользователя
                TokenGenerator generator = new();
                dbRegistrationUsers.ActivationString = generator.Generate();

                // отправка сообщения по электронной почте
                emailService.SendEmailAsync(Value.Email,
                    "Подтверждение регистрации аккаунта " + configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>Подтверждение регистрации аккаунта на " + configuration["Organization"] + "</h4>" +
                    $"<p>Ваша почта была указана в процессе регистрации нового аккаунта на сервисе " + configuration["Organization"] + ".</p>" +
                    $"<p>Если вы не регистрировались на данном сервисе просто проигнорируйте данное сообщение.</p>" +
                    $"<p>Для подтверждения регистрации пожалуйста кликните данную ссылку: " + configuration["ServerUrl"] + "/user/create?id=" + dbRegistrationUsers.ActivationString + " </p>" +
                    $"<div/>");

                // добавление в базу данных
                dbService.UsersRegistration.Create(dbRegistrationUsers);
                await dbService.SaveAsync();

                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "Заявка на регистрацию принята!",
                    Text = "На вашу электронную почту отправлено письмо. Перейдите по ссылке в письме для подтверждения вашей электронной почты.",
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

        public class Model
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9_]{3,50}")]
            public string Login { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }
    }
}
