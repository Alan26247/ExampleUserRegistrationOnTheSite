using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
using Site.Services.DataBaseServices.UsersService;
using Site.Sources;

namespace Site.Pages.User
{
    public class RemoveModel : PageModel
    {
        public RemoveModel(IConfiguration configuration, IUserService dataBaseService)
        {
            Configuration = configuration;
            this.dbService = dataBaseService;
        }

        public readonly IConfiguration Configuration;
        readonly IUserService dbService;

        public async Task<ActionResult> OnGet(string id)
        {
            if (id.Length > 256) return StatusCode(406);

            id = Cleaner.СlearWord(id);

            try
            {
                // проверяем есть ли такой активационный код
                UsersDeleted item = dbService.UsersDeleted.GetList().Where(p => p.ActivationString == id).FirstOrDefault();
                if (item == null) return StatusCode(406);

                // проверяем не просрочился ли код
                if (DateTime.Now.Subtract(item.DateRegistration).TotalHours > 3)
                {
                    // просрочился удаляем его и возвращаем статус код
                    dbService.UsersDeleted.Delete(item.Id);
                    await dbService.SaveAsync();

                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "Ошибка удаления.",
                        Text = "Ссылка по которой вы перешли ошибочна либо просрочилась. Пройдите регистрацию на удаление заново..",
                        RedirectPage = "/user/login"
                    });
                }

                Users dbUser = dbService.Users.GetList().Where(p => p.Email == item.Email).FirstOrDefault();

                //----- регистрируем удаление
                UsersRemote dbUserRemote = new()
                {
                    Email = item.Email,
                    Login =  dbUser.Login,
                    DateTimeRegistration = item.DateRegistration,
                    DeleteDate = DateTime.Now,
                    ReasonForDeletion = item.ReasonForDeletion,
                    Blocked = dbUser.Blocked
                };

                // добавление в базу данных удаленных
                dbService.UsersRemoted.Create(dbUserRemote);

                // удаление пользователя
                dbService.Users.Delete(dbUser.Id);
                await dbService.SaveAsync();

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "Аккаунт полностью удален",
                    Text = "Ваш аккаунт и все данные касаемые вашего аккаунта полностью удалены без возможности восстановления.",
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
