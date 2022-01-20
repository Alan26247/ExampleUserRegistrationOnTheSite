using System;

namespace Site.Services.DataBaseServices.Tables
{
    public class Users
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }

        public DateTime DateTimeRegistration { get; set; }      // дата и время регистрации
        public DateTime DateTimeLastVisit { get; set; }         // последний визит

        public string PasswordHash { get; set; }                // пароль хранится в хеш
        public string Token { get; set; }

        public bool Blocked { get; set; }                       // если пользователь заблокирован то true
    }
}

