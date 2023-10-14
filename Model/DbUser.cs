using System;
using Xk7.Helper.Enums;

namespace Xk7.Model
{
	internal class DbUser : IDbUser, ICloneable
	{
		private UserRole _idUserRole = UserRole.User;
		public uint IdUserRole
		{
			get => (uint)_idUserRole;
			set
			{
				_idUserRole = (UserRole)value;
			}
		}
		public string Login { get; set; } = string.Empty;
		private string _password = string.Empty;
		public string Password
		{
			get => _password;
			set
			{
				_password = value;
				HashPassword = new HashedValue(_password).ByteArray;
			}
		}
		public byte[] HashPassword { get; set; } = Array.Empty<byte>();
		public string FirstName { get; set; } = string.Empty;
		public string SecondName { get; set; } = string.Empty;
		public DateTime? DateBirthday { get; set; } = null;
		public string? DateBirthdayString
		{
			get => DateBirthday?.ToString(IDbUser.DbDateFormat);
			set
			{
				if (DateTime.TryParse(value, out DateTime dateResult))
                    DateBirthday = dateResult;
				else
                    DateBirthday = null;
            }
        }
		public bool IsBlocked { get; set; } = false;
		public DbUser() { }
        public DbUser(uint idUserRole, string login, byte[] hashPassword, string firstName, string secondName, DateTime? dateBirthday, bool isBlocked)
		{
			IdUserRole = idUserRole;
			Login = login;
			HashPassword = hashPassword;
			FirstName = firstName;
			SecondName = secondName;
			DateBirthday = dateBirthday;
			IsBlocked = isBlocked;
		}
		public bool AllFieldsFilled() => Enum.IsDefined(typeof(UserRole), (int)IdUserRole) && Login != string.Empty && HashPassword != null
            && HashPassword.Length != 0 && FirstName != string.Empty && SecondName != string.Empty && DateBirthday != null;
        public object Clone() => new DbUser(IdUserRole, Login, HashPassword, FirstName, SecondName, DateBirthday, IsBlocked);
	}
}