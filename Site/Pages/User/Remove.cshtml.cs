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

            id = Cleaner.�learWord(id);

            try
            {
                // ��������� ���� �� ����� ������������� ���
                UsersDeleted item = dbService.UsersDeleted.GetList().Where(p => p.ActivationString == id).FirstOrDefault();
                if (item == null) return StatusCode(406);

                // ��������� �� ����������� �� ���
                if (DateTime.Now.Subtract(item.DateRegistration).TotalHours > 3)
                {
                    // ����������� ������� ��� � ���������� ������ ���
                    dbService.UsersDeleted.Delete(item.Id);
                    await dbService.SaveAsync();

                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "������ ��������.",
                        Text = "������ �� ������� �� ������� �������� ���� ������������. �������� ����������� �� �������� ������..",
                        RedirectPage = "/user/login"
                    });
                }

                Users dbUser = dbService.Users.GetList().Where(p => p.Email == item.Email).FirstOrDefault();

                //----- ������������ ��������
                UsersRemote dbUserRemote = new()
                {
                    Email = item.Email,
                    Login =  dbUser.Login,
                    DateTimeRegistration = item.DateRegistration,
                    DeleteDate = DateTime.Now,
                    ReasonForDeletion = item.ReasonForDeletion,
                    Blocked = dbUser.Blocked
                };

                // ���������� � ���� ������ ���������
                dbService.UsersRemoted.Create(dbUserRemote);

                // �������� ������������
                dbService.Users.Delete(dbUser.Id);
                await dbService.SaveAsync();

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "������� ��������� ������",
                    Text = "��� ������� � ��� ������ �������� ������ �������� ��������� ������� ��� ����������� ��������������.",
                    RedirectPage = "/user/login"
                });
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
    }
}
