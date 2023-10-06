using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xk7.Helper.Enums;

namespace Xk7.Model
{
    internal class DbTest
    {
        protected uint _id;
        protected string _tittle;
        protected string _description;
        protected DateTime _utcDateTimeCreated;
        protected string _whoCreated;
        protected bool _hasImage;
        public uint Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Tittle
        {
            get
            {
                return _tittle;
            }
            set
            {
                _tittle = value;
                OnPropertyChanged("Tittle");
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }
        public DateTime UtcDateTimeCreated
        {
            get
            {
                return _utcDateTimeCreated;
            }
            set
            {
                _utcDateTimeCreated = value;
                OnPropertyChanged("UtcDateTimeCreated");
            }
        }
        public string WhoCreated
        {
            get
            {
                return _whoCreated;
            }
            set
            {
                _whoCreated = value;
                OnPropertyChanged("WhoCreated");
            }
        }
        public bool HasImage
        {
            get
            {
                return _hasImage;
            }
            set
            {
                _hasImage = value;
                OnPropertyChanged("HasImage");
            }
        }
        internal DbTest() { }
        internal DbTest(uint id, string tittle, string description, DateTime utcDateTimeCreated, string whoCreated, bool hasImage)
        {
            _id = id;
            _tittle = tittle;
            _description = description;
            _utcDateTimeCreated = utcDateTimeCreated;
            _whoCreated = whoCreated;
            _hasImage = hasImage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    internal class DbTestWithImage : DbTest
    {
        protected string _pathImage;
        public string PathImage
        {
            get
            {
                return _pathImage;
            }
            set
            {
                _pathImage = value;
                OnPropertyChanged("PathImage");
            }
        }
        internal DbTestWithImage() { }
        internal DbTestWithImage(uint id, string tittle, string description, DateTime utcDateTimeCreated, string pathImage)
        {
            _id = id;
            _tittle = tittle;
            _description = description;
            _utcDateTimeCreated = utcDateTimeCreated;
            _hasImage = true;
            _pathImage = pathImage;
        }
    }
}
