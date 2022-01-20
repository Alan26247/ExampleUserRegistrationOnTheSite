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

            id = Cleaner.�learWord(id);

            try
            {
                // ��������� ���� �� ����� ������������� ���
                UsersRecovery item = dbService.UsersRecovery.GetList().Where(p => p.ActivationString == id).FirstOrDefault();
                if (item == null) return StatusCode(406);

                // ��������� �� ����������� �� ���
                if (DateTime.Now.Subtract(item.DateRegistration).TotalHours > 3)
                {
                    // ����������� ������� ��� � ���������� ������ ���
                    dbService.UsersRegistration.Delete(item.Id);
                    await dbService.SaveAsync();

                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "������ �������������� ��������.",
                        Text = "������ �� ������� �� ������� �������� ���� ������������. �������� ����������� �������������� ������..",
                        RedirectPage = "/user/login"
                    });
                }

                Users dbUser = dbService.Users.GetList().Where(p => p.Email == item.Email).FirstOrDefault();

                // ������� ����� ������
                PasswordGenerator passwordGenerator = new ();
                string newPassword = passwordGenerator.Generate(12);

                // ��������� ��� ������
                CalculateHashString calculateHash = new();
                dbUser.PasswordHash = calculateHash.Calculate(newPassword);

                await dbService.SaveAsync();

                // ���������� ������
                emailService.SendEmailAsync(item.Email,
                    "���������� �� �������������� �������� " + Configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>���������� �� �������������� �������� �� " + Configuration["Organization"] + "</h4>" +
                    $"<p>��� ������� ������� ������������! .</p>" +
                    $"<br />" +
                    $"<p>������ ��� �����:</p>" +
                    $"<p>����������� �����: " + item.Email + "</p>" +
                    $"<p>������:            " + newPassword + "</p>" +
                    $"<br />" +
                    $"<p>��� ����� �� ��������� �� ������: " + Configuration["ServerUrl"] + "/user/login </p>" +
                    $"<div/>");

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "������� ������� ������������!",
                    Text = "�� ���� ����������� ����� ���� ���������� ��������� � ����� �������.",
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
