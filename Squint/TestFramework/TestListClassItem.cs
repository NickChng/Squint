using System;
using SquintScript.ViewModels;
using SquintScript.Extensions;

namespace SquintScript.TestFramework
{
    public abstract class TestListClassItem<T> : ObservableObject where T : class
    {
        public CheckTypes CheckType { get; set; }

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
                if (Reference == null)
                    return _EmptyRefValueString;
                if (!Reference.isDefined)
                    return _EmptyRefValueString;
                if (Check == null)
                    return _EmptyCheckValueString;
                if (Check is double)
                {
                    if (double.IsNaN(Convert.ToDouble(Check)))
                        return _EmptyCheckValueString;
                    else
                        return _WarningString;
                }
                else
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

