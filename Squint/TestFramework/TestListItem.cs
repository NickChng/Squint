using Squint.ViewModels;
using Squint.Extensions;

namespace Squint.TestFramework
{
    public abstract class TestListItem<T> : ObservableObject
    {
        public CheckTypes CheckType { get; set; }
        public bool IsInfoOnly { get; set; } = false;
        public string OptionalNameSuffix { get; set; } = "";
        public string CheckTypeString { get { return string.Format("{0} {1}", CheckType.Display(), OptionalNameSuffix); } }
        public TrackedValue<T> Reference { get; set; }
        public TrackedValue<T> Tolerance { get; set; }

        public T Check { get; set; }

        protected string _EmptyCheckValueString = "Check value undefined";
        protected string _EmptyRefValueString = "No reference";
        protected string _WarningString = "Warning";

        public virtual string WarningString
        {
            get
            {
                return _WarningString;
            }
            set
            {
                _WarningString = value;
            }
        }
    }

}
//}

