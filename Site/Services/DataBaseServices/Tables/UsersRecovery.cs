using System;

namespace Site.Services.DataBaseServices.Tables
{
    // таблица хранящая пользователей зарегистрированных для восстановления пароля
    public class UsersRecovery
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public string ActivationString { get; set; }   // строка в 32 символа для подтверждения
        public DateTime DateRegistration { get; set; } // время регистрации запроса 
        public int UserId { get; set; }              // внещний ключ
        public Users User { get; set; }
    }
}
