using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xk7.Helper;
using Xk7.Helper.Converts;

namespace Xk7.Model
{
    internal class DbUser : IDbUser, ICloneable, INotifyPropertyChanged
    {
        private uint idUserRole;
        private string login;
        private byte[] hashPassword;
        private string firstName;
        private string secondName;
        private DateTime dateBirthday;
        private bool isBlocked;
        public uint IdUserRole
        {
            get
            {
                return idUserRole;
            }
            set
            {
                idUserRole = value;
                OnPropertyChanged("IdTimetable");
            }
        }

        public string? Login
        {
            get
            {
                return login;
            }
            set
            {
                login = value;
                OnPropertyChanged("EmployeeLogin");
            }
        }
        public byte[] HashPassword
        {
            get
            {
                return hashPassword;
            }
            set
            {
                hashPassword = value;
                OnPropertyChanged("SlotDate");
            }
        }
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value;
                OnPropertyChanged("SlotTime");
            }
        }

        public string SecondName
        {
            get
            {
                return secondName;
            }
            set
            {
                secondName = value;
                OnPropertyChanged("SlotTime");
            }
        }
        public DateTime DateBirthday
        {
            get
            {
                return dateBirthday;
            }
            set
            {
                dateBirthday = value;
                OnPropertyChanged("SlotTime");
            }
        }

        public bool IsBlocked
        {
            get
            {
                return isBlocked;
            }
            set
            {
                isBlocked = value;
                OnPropertyChanged("SlotTime");
            }
        }

        public DbUser() {}
        public DbUser(uint idUserRole, string login, byte[] hashPassword, string firstName, string secondName,
            DateTime dateBirthday, bool isBlocked)
        {
            IdUserRole = idUserRole;
            Login = login;
            HashPassword = hashPassword;
            FirstName = firstName;
            SecondName = secondName;
            DateBirthday = dateBirthday;
            IsBlocked = isBlocked;
        }

        public DbUser(User user)
        {
            IdUserRole = (uint)user.IdUserRole;
            Login = user.Login;
            HashPassword = Encoding.UTF8.GetBytes(user.Password.Value);
            FirstName = firstName;
            SecondName = secondName;
            DateBirthday = new DateTime(user.DateBirthday.Year, user.DateBirthday.Month, user.DateBirthday.Day, 0, 0, 0);
            IsBlocked = isBlocked;
        }

        public override string ToString()
        {
            return
                $"[ IdUserRole: {IdUserRole.ToString()}, Login: {Login}, HashPassword: {Converts.ConvertByteArrayToString(HashPassword)}, FirstName: {FirstName}, " +
                $"SecondName: {SecondName}, DateBirthday: {DateBirthday}, IsBlocked: {IsBlocked}]";
        }

        public static bool operator ==(DbUser user1, DbUser user2)
        {
            return user1.IdUserRole == user2.IdUserRole && user1.IsBlocked == user2.IsBlocked && user1.Login == user2.Login
                && user1.HashPassword == user2.HashPassword && user1.FirstName == user2.FirstName && user1.SecondName == user2.SecondName
                && user1.DateBirthday == user2.DateBirthday;
        }

        public static bool operator !=(DbUser user1, DbUser user2)
        {
            return !(user1.IdUserRole == user2.IdUserRole && user1.IsBlocked == user2.IsBlocked && user1.Login == user2.Login
                && user1.HashPassword == user2.HashPassword && user1.FirstName == user2.FirstName && user1.SecondName == user2.SecondName
                && user1.DateBirthday == user2.DateBirthday);
        }

        public object Clone()
        {
            return new DbUser(IdUserRole, Login, HashPassword, FirstName, SecondName, DateBirthday, IsBlocked);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
