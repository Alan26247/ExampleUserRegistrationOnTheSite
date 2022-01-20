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
    public class RegistrationRecoveryModel : PageModel
    {
        public RegistrationRecoveryModel(IConfiguration configuration, IUserService dataBaseService,
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

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost()
        {
            // проверка валидации
            if (!ModelState.IsValid) return Page();

            Value.Email = Cleaner.СlearSpaces(Value.Email);   // удаление пробелов

            try
            {
                // Ищем есть ли пользователь с такой электронной почтой?
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

                // проверяем не отправляли ли мы уже запрос на подтверждение данному пользователю
                UsersRecovery item = dbService.UsersRecovery.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (item != null)
                {
                    // проверяем не просрочился ли код
                    if (DateTime.Now.Subtract(item.DateRegistration).TotalHours > 3)
                    {
                        // просрочился удаляем его
                        dbService.UsersRegistration.Delete(item.Id);
                    }
                    else
                    {
                        return RedirectToPage("/info", new InfoModel.Model()
                        {
                            Header = "Ошибка регистрации восстановления аккаунта.",
                            Text = "Вы уже отправляли запрос на восстановление ранее. На вашу электронную почту ранее уже отправлялось письмо. Для подтверждения восстановления перейдите по ссылке в письме.",
                            RedirectPage = "/user/login"
                        });
                    }
                }

                // создание регистрации пользователя
                item = new UsersRecovery
                {
                    Email = Value.Email,
                    DateRegistration = DateTime.Now,
                    UserId = dbUser.Id
                };


                // здесь генерация нового кода и отправка его на email пользователя
                TokenGenerator generator = new();
                item.ActivationString = generator.Generate();

                // отправка сообщения по электронной почте
                emailService.SendEmailAsync(Value.Email,
                    "Подтверждение восстановления аккаунта " + configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>Подтверждение восстановления аккаунта на " + configuration["Organization"] + "</h4>" +
                    $"<p>Вы регистрировали запрос на восстановление аккаунта на сервисе " + configuration["Organization"] + ".</p>" +
                    $"<p>Если вы не регистрировали запрос либо вспомнили учетные данные просто проигнорируйте данное сообщение.</p>" +
                    $"<p>Для подтверждения восстановления пожалуйста кликните данную ссылку: " + configuration["ServerUrl"] + "/user/recovery?id=" + item.ActivationString + " </p>" +
                    $"<div/>");

                // добавление в базу данных
                dbService.UsersRecovery.Create(item);
                await dbService.SaveAsync();

                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "Заявка на восстановление аккаунта принята.",
                    Text = "На вашу электронную почту отправлено письмо. Перейдите по ссылке в письме для подтверждения восстановления аккаунта.",
                    RedirectPage = "/user/login"
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
        }
    }
}
