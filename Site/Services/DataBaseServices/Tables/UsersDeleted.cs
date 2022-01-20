using System;

namespace Site.Services.DataBaseServices.Tables
{
    // таблица хранящая пользователей зарегистрированных для удаления
    public class UsersDeleted
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string ReasonForDeletion { get; set; }

        public string ActivationString { get; set; }   // строка символов для подтверждения email
        public DateTime DateRegistration { get; set; } // время регистрации запроса на удаление
        public int UserId { get; set; }              // внещний ключ
        public Users User { get; set; }
    }
}
