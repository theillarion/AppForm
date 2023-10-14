using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Enums
{
    internal enum LoadFileResult
    {
        Success,
        SourceFileNotExists,
        DestinationFileExists,
        Unknown
    }
    internal enum LoadImageResult
    {
        Success,
        IdNotExists,
        SourceImageNotExists,
        DestinationImageExists,
        Unknown
    }
}
