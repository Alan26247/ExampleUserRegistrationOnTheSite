using System;

namespace Site.Services.DataBaseServices.Tables
{
    // таблица хранящая удаленных пользователей
    public class UsersRemote
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public string Login { get; set; }
        public DateTime DateTimeRegistration { get; set; }      // дата и время регистрации

        public bool Blocked { get; set; }                       // если пользователь заблокирован то true
        public DateTime DeleteDate;                             // дата в время удаления
        public string ReasonForDeletion { get; set; }
    }
}
