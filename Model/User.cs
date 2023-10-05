using Xk7.Helper.Enums;
using System;
using System.ComponentModel;

namespace Xk7.Model
{
    public class User
    {
        public UserRole IdUserRole;
        public string Login { get; set; }
        public HashedValue Password { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateOnly DateBirthday { get; set; }
        public bool IsBanned { get; set; } = false;
        public User() { }
        public User(UserRole userRole, string login, HashedValue password, string firstName, string secondName, DateOnly dateBirthday, bool isBanned)
        {
            IdUserRole = userRole;
            Login = login;
            Password = password;
            FirstName = firstName;
            SecondName = secondName;
            DateBirthday = dateBirthday;
            IsBanned = isBanned;
        }


        public override string ToString()
        {
            return $"[ IdUserRole: {IdUserRole}, Login: {Login}, Password: {Password}, FirstName: {FirstName}, " +
                   $"SecondName: {SecondName}, DateBirthday: {DateBirthday}, IsBanned: {IsBanned}]";
        }


    }
}
