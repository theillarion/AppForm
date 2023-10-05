using System.Text;
using Xk7.Helper.Converts;

namespace Xk7.Model
{
    public class HashedValue
    {
        private string _originalValue;
        public string OriginalValue
        {
            get => _originalValue;
            set
            {
                _originalValue = value;
                Value = Converts.ConvertStringToHeshString(value);
            }
        }
        public string Value { get; private set; }

        private HashedValue() { }
        public HashedValue(string originalValue)
        {
            OriginalValue = originalValue;
        }
        public override string ToString()
        {
            return Value;
        }
    }
}
