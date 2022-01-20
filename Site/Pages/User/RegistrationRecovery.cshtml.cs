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
            // �������� ���������
            if (!ModelState.IsValid) return Page();

            Value.Email = Cleaner.�learSpaces(Value.Email);   // �������� ��������

            try
            {
                // ���� ���� �� ������������ � ����� ����������� ������?
                Users dbUser = dbService.Users.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (dbUser == null)
                {
                    return RedirectToPage("/info", new InfoModel.Model()
                    {
                        Header = "������ �������������� ��������.",
                        Text = "������������ � ����� ������� ����������� ����� �� ���������������.",
                        RedirectPage = "/user/login"
                    });
                }

                // ��������� �� ���������� �� �� ��� ������ �� ������������� ������� ������������
                UsersRecovery item = dbService.UsersRecovery.GetList().Where(p => p.Email == Value.Email).FirstOrDefault();
                if (item != null)
                {
                    // ��������� �� ����������� �� ���
                    if (DateTime.Now.Subtract(item.DateRegistration).TotalHours > 3)
                    {
                        // ����������� ������� ���
                        dbService.UsersRegistration.Delete(item.Id);
                    }
                    else
                    {
                        return RedirectToPage("/info", new InfoModel.Model()
                        {
                            Header = "������ ����������� �������������� ��������.",
                            Text = "�� ��� ���������� ������ �� �������������� �����. �� ���� ����������� ����� ����� ��� ������������ ������. ��� ������������� �������������� ��������� �� ������ � ������.",
                            RedirectPage = "/user/login"
                        });
                    }
                }

                // �������� ����������� ������������
                item = new UsersRecovery
                {
                    Email = Value.Email,
                    DateRegistration = DateTime.Now,
                    UserId = dbUser.Id
                };


                // ����� ��������� ������ ���� � �������� ��� �� email ������������
                TokenGenerator generator = new();
                item.ActivationString = generator.Generate();

                // �������� ��������� �� ����������� �����
                emailService.SendEmailAsync(Value.Email,
                    "������������� �������������� �������� " + configuration["Organization"] + ".",
                    $"<div>" +
                    $"<h4>������������� �������������� �������� �� " + configuration["Organization"] + "</h4>" +
                    $"<p>�� �������������� ������ �� �������������� �������� �� ������� " + configuration["Organization"] + ".</p>" +
                    $"<p>���� �� �� �������������� ������ ���� ��������� ������� ������ ������ �������������� ������ ���������.</p>" +
                    $"<p>��� ������������� �������������� ���������� �������� ������ ������: " + configuration["ServerUrl"] + "/user/recovery?id=" + item.ActivationString + " </p>" +
                    $"<div/>");

                // ���������� � ���� ������
                dbService.UsersRecovery.Create(item);
                await dbService.SaveAsync();

                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "������ �� �������������� �������� �������.",
                    Text = "�� ���� ����������� ����� ���������� ������. ��������� �� ������ � ������ ��� ������������� �������������� ��������.",
                    RedirectPage = "/user/login"
                });
            }
            catch
            {
                return RedirectToPage("/info", new InfoModel.Model()
                {
                    Header = "������ �������� ����������.",
                    Text = "������ ��������� �� ��������� ����������.",
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
