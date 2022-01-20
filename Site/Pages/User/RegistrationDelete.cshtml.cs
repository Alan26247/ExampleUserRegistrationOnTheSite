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
    public class RegistrationDeleteModel : PageModel
    {
        public RegistrationDeleteModel(IConfiguration configuration, IUserService dataBaseService,
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
            Value = new Model();
            if (Request.Cookies["Email"] != null) Value.Email = Request.Cookies["Email"];
        }

        public async Task<ActionResult> OnPost()
        {
            // проверка валидации
            if (!ModelState.IsValid) return Page();

            Value.Email = Cleaner.СlearSpaces(Value.Email);   // удаление пробелов
            if (Value.ReasonForDeletion == null) Value.ReasonForDeletion = "";

            try
            {
                // Ищем есть ли пользователь с такой электронной почтой?
                Users dbUser = dbService.Users.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (dbUser == null)
                {
                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Ошибка удаления аккаунта.",
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
                        Header = "Ошибка удаления аккаунта.",
                        Text = "Ошибка пароля",
                        RedirectPage = "/user/login"
                    });
                }

                // проверяем не отправляли ли мы уже запрос на подтверждение данному пользователю
                UsersDeleted item = dbService.UsersDeleted.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
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
                            Header = "Ошибка регистрации удаления.",
                            Text = "Вы уже отправляли запрос на удаление ранее. На вашу электронную почту ранее уже отправлялось письмо. Для подтверждения удаления  перейдите по ссылке в письме.",
                            RedirectPage = "/main"
                        });
                    }
                }

                // создание регистрации пользователя
                item = new UsersDeleted
                {
                    Email = Value.Email,
                    ReasonForDeletion = Value.ReasonForDeletion,
                    DateRegistration = DateTime.Now,
                    UserId = dbUser.Id
                };

                // здесь генерация нового кода и отправка его на email пользователя
                TokenGenerator generator = new();
                item.ActivationString = generator.Generate();

                // отправка сообщения по электронной почте
                emailService.SendEmailAsync(Value.Email,
                    "Подтверждение удаления аккаунта " + configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>Подтверждение удаления аккаунта на " + configuration["Organization"] + "</h4>" +
                    $"<p>Вы регистрировали запрос на удаление аккаунта с сервиса " + configuration["Organization"] + ".</p>" +
                    $"<p>Если вы не регистрировали запрос либо передумали удалять аккаунт просто проигнорируйте данное сообщение.</p>" +
                    $"<p>Для подтверждения удаления пожалуйста кликните данную ссылку: " + configuration["ServerUrl"] + "/user/remove?id=" + item.ActivationString + " </p>" +
                    $"<div/>");

                // добавление в базу данных
                dbService.UsersDeleted.Create(item);
                await dbService.SaveAsync();

                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "Заявка на удаление принята.",
                    Text = "На вашу электронную почту отправлено письмо. Перейдите по ссылке в письме для подтверждения удаления аккаунта с сервиса.",
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
            [RegularExpression(@"[A-zА-я0-9\s]{0,1024}")] 
            public string ReasonForDeletion { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string Password { get; set; }
        }
    }
}
