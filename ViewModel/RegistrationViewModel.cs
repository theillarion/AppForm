using System.ComponentModel;
using System.Windows.Media;

using Xk7.Model;

namespace Xk7.ViewModel
{
	internal class RegistrationViewModel : INotifyPropertyChanged
	{
		public static readonly string NoError = string.Empty;
		private IDbUser _user;
		public IDbUser User
		{
			get { return _user; }
			set
			{
				if (_user != value)
				{
					_user = value;
					OnPropertyChanged(nameof(User));
				}
			}
		}
		private string _errorFirstName = NoError;
		public string ErrorFirstName
		{
			get { return _errorFirstName; }
			set
			{
				_errorFirstName = value;
				if (value == NoError)
				{
					ToolTipFirstNameEnabled = false;
					BorderFirstName = Brushes.Gray;
				}
				else
				{
					ToolTipFirstNameEnabled = true;
					BorderFirstName = Brushes.Red;
				}
				OnPropertyChanged(nameof(ErrorFirstName));
			}
		}
		private Brush _borderFirstName;
		public Brush BorderFirstName
		{
			get { return _borderFirstName; }
			set
			{
				_borderFirstName = value;
				OnPropertyChanged(nameof(BorderFirstName));
			}
		}
		private bool _toolTipFirstNameEnabled;
		public bool ToolTipFirstNameEnabled
		{
			get { return _toolTipFirstNameEnabled; }
			set
			{
				_toolTipFirstNameEnabled = value;
				OnPropertyChanged(nameof(ToolTipFirstNameEnabled));
			}
		}
		private string _errorSecondName = NoError;
		public string ErrorSecondName
		{
			get { return _errorSecondName; }
			set
			{
				_errorSecondName = value;
				if (value == NoError)
				{
					ToolTipSecondNameEnabled = false;
					BorderSecondName = Brushes.Gray;
				}
				else
				{
					ToolTipSecondNameEnabled = true;
					BorderSecondName = Brushes.Red;
				}
				OnPropertyChanged(nameof(ErrorSecondName));
			}
		}
		private Brush _borderSecondName;
		public Brush BorderSecondName
		{
			get { return _borderSecondName; }
			set
			{
				_borderSecondName = value;
				OnPropertyChanged(nameof(BorderSecondName));
			}
		}
		private bool _toolTipSecondNameEnabled;
		public bool ToolTipSecondNameEnabled
		{
			get { return _toolTipSecondNameEnabled; }
			set
			{
				_toolTipSecondNameEnabled = value;
				OnPropertyChanged(nameof(ToolTipSecondNameEnabled));
			}
		}
		private string _errorLogin = NoError;
		public string ErrorLogin
		{
			get { return _errorLogin; }
			set
			{
				_errorLogin = value;
				if (value == NoError)
				{
					ToolTipLoginEnabled = false;
					BorderLogin = Brushes.Gray;
				}
				else
				{
					ToolTipLoginEnabled = true;
					BorderLogin = Brushes.Red;
				}
				OnPropertyChanged(nameof(ErrorLogin));
			}
		}
		private Brush _borderLogin;
		public Brush BorderLogin
		{
			get { return _borderLogin; }
			set
			{
				_borderLogin = value;
				OnPropertyChanged(nameof(BorderLogin));
			}
		}
		private bool _toolTipLoginEnabled;
		public bool ToolTipLoginEnabled
		{
			get { return _toolTipLoginEnabled; }
			set
			{
				_toolTipLoginEnabled = value;
				OnPropertyChanged(nameof(ToolTipLoginEnabled));
			}
		}

		private string _errorPassword = NoError;
		public string ErrorPassword
		{
			get { return _errorPassword; }
			set
			{
				_errorPassword = value;
				if (value == NoError)
				{
					ToolTipPasswordEnabled = false;
					BorderPassword = Brushes.Gray;
				}
				else
				{
					ToolTipPasswordEnabled = true;
					BorderPassword = Brushes.Red;
				}
				OnPropertyChanged(nameof(ErrorPassword));
			}
		}
		private Brush _borderPassword;
		public Brush BorderPassword
		{
			get { return _borderPassword; }
			set
			{
				_borderPassword = value;
				OnPropertyChanged(nameof(BorderPassword));
			}
		}
		private bool _toolTipPasswordEnabled;
		public bool ToolTipPasswordEnabled
		{
			get => _toolTipPasswordEnabled;
			set
			{
				_toolTipPasswordEnabled = value;
				OnPropertyChanged(nameof(ToolTipPasswordEnabled));
			}
		}
		private string _errorDateBirthday;
		public string ErrorDateBirthday
		{
			get { return _errorDateBirthday; }
			set
			{
				_errorDateBirthday = value;
				if (value == NoError)
				{
					ToolTipDateBirthdayEnabled = false;
					BorderDateBirthday = Brushes.Gray;
				}
				else
				{
					ToolTipDateBirthdayEnabled = true;
					BorderDateBirthday = Brushes.Red;
				}
				OnPropertyChanged(nameof(ErrorDateBirthday));
			}
		}
		private Brush _borderDateBirthday;
		public Brush BorderDateBirthday
		{
			get { return _borderDateBirthday; }
			set
			{
				_borderDateBirthday = value;
				OnPropertyChanged(nameof(BorderDateBirthday));
			}
		}
		private bool _toolTipDateBirthdayEnabled;
		public bool ToolTipDateBirthdayEnabled
		{
			get { return _toolTipDateBirthdayEnabled; }
			set
			{
				_toolTipDateBirthdayEnabled = value;
				OnPropertyChanged(nameof(ToolTipDateBirthdayEnabled));
			}
		}
		public bool AllCorrect() => User.AllFieldsFilled() && ErrorFirstName == NoError
			&& ErrorSecondName == NoError && ErrorLogin == NoError && ErrorPassword == NoError && ErrorDateBirthday == NoError;
		public RegistrationViewModel() : this(new DbUser()) { }
        public RegistrationViewModel(IDbUser user)
        {
			User = user;
            ErrorFirstName = NoError;
            ErrorSecondName = NoError;
            ErrorLogin = NoError;
            ErrorPassword = NoError;
            ErrorDateBirthday = NoError;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}