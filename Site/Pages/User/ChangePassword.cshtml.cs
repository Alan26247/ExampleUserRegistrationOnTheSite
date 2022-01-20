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
    public class ChangePasswordModel : PageModel
    {
        public ChangePasswordModel(IUserService dataBaseService)
        {
            dbService = dataBaseService;
        }

        readonly IUserService dbService;
        [BindProperty]
        public Model Value { get; set; }

        public ActionResult OnGet()
        {
            Value = new Model();
            string cookieEmail = Request.Cookies["Email"];

            if (cookieEmail != null)
            {
                Value.Email = cookieEmail;
            }
            else return RedirectToAction("/user/login");

            return Page();
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

                // ��������� ������
                CalculateHashString calculateHash = new();
                if (dbUser.PasswordHash != calculateHash.Calculate(Value.CurrentPassword))
                {
                    return RedirectToPage("/Info", new InfoModel.Model()
                    {
                        Header = "������ ��������� ������.",
                        Text = "������� ������ ������ �������.",
                        RedirectPage = "/user/changepassword"
                    });
                }

                // ��������� ����� ��� ������ ������
                dbUser.PasswordHash = calculateHash.Calculate(Value.NewPassword);

                await dbService.SaveAsync();

                return RedirectToPage("/Info", new InfoModel.Model()
                {
                    Header = "������ �������.",
                    Text = "",
                    RedirectPage = "/main"
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
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string NewPassword { get; set; }
            [Required]
            [RegularExpression(@"[A-z0-9]{6,256}")]
            public string CurrentPassword { get; set; }
        }
    }
}
