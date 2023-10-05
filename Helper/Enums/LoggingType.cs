using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Enums
{
    public enum LoggingType
    {
        SuccessRegistration = 1,
        SuccessAuthentication,
        FailedAuthenticationWrongPassword,
        FailedAuthenticationUserBanned,
    }
}
