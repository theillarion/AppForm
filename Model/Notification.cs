using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Model
{
    public class Notification : INotifyPropertyChanged
    {
        private DateTime dateTimeCreated;
        public DateTime DateTimeCreated
        {
            get { return dateTimeCreated; }
            set
            {
                dateTimeCreated = value;
                OnPropertyChanged("DateTimeCreated");
            }
        }
        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        public Notification()
        {

        }
        public Notification(string description, DateTime dateTimeCreated, bool isChecked)
        {
            this.description = description;
            this.dateTimeCreated = dateTimeCreated;
            this.isChecked = isChecked;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}