using System;

namespace Site.Services.DataBaseServices.Tables
{
    // таблица хранящая пользователей для подтверждения регистрации через почту
    public class UsersRegistration
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime DateRegistration { get; set; }
        public string ActivationString { get; set; }
    }
}
