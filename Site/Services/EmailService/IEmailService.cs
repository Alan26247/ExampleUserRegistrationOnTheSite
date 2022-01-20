using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Site.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmailAsync(string email, string subject, string body);
    }
}
