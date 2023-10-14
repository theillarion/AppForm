using System;

namespace Xk7.Model
{
    public interface IDbUser : IDbBase
    {
        public static string DbDateFormat = "yyyy-MM-dd";
        uint IdUserRole { get; set; }
        string Login { get; set; }
        byte[] HashPassword { get; set; }
        string FirstName { get; set; }
        string SecondName { get; set; }
        DateTime? DateBirthday { get; set; }
        bool IsBlocked { get; set; }
    }
}