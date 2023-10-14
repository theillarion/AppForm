using System;
using System.Text;
using Xk7.Helper.Converts;

namespace Xk7.Model
{
    public class HashedValue
    {
        private string _originalValue = string.Empty;
        public string OriginalValue
        {
            get => _originalValue;
            set
            {
                _originalValue = value;
                Value = Converts.ConvertStringToHeshString(value);
                ByteArray = Encoding.UTF8.GetBytes(value);
            }
        }
        public string Value { get; private set; } = string.Empty;
        public byte[] ByteArray { get; private set; } = Array.Empty<byte>();
        public HashedValue(string originalValue)
        {
            OriginalValue = originalValue;
        }
    }
}