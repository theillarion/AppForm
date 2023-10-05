using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Model
{
    public interface IDbUser
    {
        public uint IdUserRole { get; set; }
        public string Login { get; set; }
        public byte[] HashPassword { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime DateBirthday { get; set; }
        public bool IsBlocked { get; set; }
    }
}
