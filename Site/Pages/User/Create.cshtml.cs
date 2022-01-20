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

            id = Cleaner.�learWord(id);

            try
            {
                // ��������� ���� �� ����� ������������� ���
                UsersRegistration dbRegistrationUser = dbService.UsersRegistration.GetList().Where(p => p.ActivationString == id).FirstOrDefault();
                if (dbRegistrationUser == null) return StatusCode(406);

                // ��������� �� ����������� �� ���
                if (DateTime.Now.Subtract(dbRegistrationUser.DateRegistration).TotalHours > 3)
                {
                    // ����������� ������� ��� � ���������� ������ ���
                    dbService.UsersRegistration.Delete(dbRegistrationUser.Id);
                    await dbService.SaveAsync();

                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "������ �����������.",
                        Text = "������ �� ������� �� ������� �������� ���� ������������. �������� ����������� ������..",
                        RedirectPage = "/user/login"
                    });
                }

                //----- ��������� ������������
                TokenGenerator tokenGenerator = new();
                Users dbUser = new()
                {
                    Email = dbRegistrationUser.Email,
                    Login = dbRegistrationUser.Login,
                    DateTimeRegistration = DateTime.Now,
                    DateTimeLastVisit = DateTime.Now,
                    Token = tokenGenerator.Generate()
                };

                // ��������� ��� ������
                CalculateHashString calculateHash = new();
                dbUser.PasswordHash = calculateHash.Calculate(dbRegistrationUser.Password);

                // ���������� � ���� ������
                dbService.Users.Create(dbUser);

                // �������� ��������������� ������� �� ���� ������
                dbService.UsersRegistration.Delete(dbRegistrationUser.Id);
                await dbService.SaveAsync();

                // ���������� ������
                emailService.SendEmailAsync(dbRegistrationUser.Email,
                    "���������� � ����������� �������� " + Configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>���������� � ����������� �������� �� " + Configuration["Organization"] + "</h4>" +
                    $"<p>��� ������� ������� ����������� �� ������� " + Configuration["Organization"] + ".</p>" +
                    $"<br />" +
                    $"<p>������ ��� �����:</p>" +
                    $"<p>����������� �����: " + dbRegistrationUser.Email + "</p>" +
                    $"<p>������:            " + dbRegistrationUser.Password + "</p>" +
                    $"<br />" +
                    $"<p>��� ����� �� ��������� �� ������: " + Configuration["ServerUrl"] + "/user/login </p>" +
                    $"<div/>");

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "������� �����������!",
                    Text = "���� ����������� ����� ������� ������������.",
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
