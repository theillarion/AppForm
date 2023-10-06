using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xk7.Helper.Converts;

namespace Xk7.Model
{
    internal class DbUser : IDbUser, ICloneable, INotifyPropertyChanged
    {
        private uint _idUserRole;
        private string _login;
        private byte[] _hashPassword;
        private string _firstName;
        private string _secondName;
        private DateTime _dateBirthday;
        private bool _isBlocked;
        public uint IdUserRole
        {
            get
            {
                return _idUserRole;
            }
            set
            {
                _idUserRole = value;
                OnPropertyChanged("IdUserRole");
            }
        }
        public string Login
        {
            get
            {
                return _login;
            }
            set
            {
                _login = value;
                OnPropertyChanged("Login");
            }
        }
        public byte[] HashPassword
        {
            get
            {
                return _hashPassword;
            }
            set
            {
                _hashPassword = value;
                OnPropertyChanged("HashPassword");
            }
        }
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value;
                OnPropertyChanged("FirstName");
            }
        }
        public string SecondName
        {
            get
            {
                return _secondName;
            }
            set
            {
                _secondName = value;
                OnPropertyChanged("SecondName");
            }
        }
        public DateTime DateBirthday
        {
            get
            {
                return _dateBirthday;
            }
            set
            {
                _dateBirthday = value;
                OnPropertyChanged("DateBirthday");
            }
        }
        public bool IsBlocked
        {
            get
            {
                return _isBlocked;
            }
            set
            {
                _isBlocked = value;
                OnPropertyChanged("IsBlocked");
            }
        }
        public DbUser() : this(0, string.Empty, new byte[0], string.Empty, string.Empty, DateTime.FromFileTimeUtc(0), false) {}
        public DbUser(uint idUserRole, string login, byte[] hashPassword, string firstName, string secondName, DateTime dateBirthday, bool isBlocked)
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
            FirstName = user.FirstName;
            SecondName = user.SecondName;
            DateBirthday = new DateTime(user.DateBirthday.Year, user.DateBirthday.Month, user.DateBirthday.Day, 0, 0, 0);
            IsBlocked = user.IsBanned;
        }
        public bool AllFieldsFilled()
        {
            return Login != string.Empty && HashPassword != null && HashPassword.Length != 0 && FirstName != string.Empty
                && SecondName != string.Empty;
        }
        public override string ToString()
        {
            return
                $"[ IdUserRole: {IdUserRole.ToString()}, Login: {Login}, HashPassword: {Converts.ConvertByteArrayToString(HashPassword)}, FirstName: {FirstName}, " +
                $"SecondName: {SecondName}, DateBirthday: {DateBirthday}, IsBlocked: {IsBlocked}]";
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
