using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SquintScript.Interfaces
{
    public interface ITestListItem
    {
        void SetCheckValue(object value);
        CheckTypes CheckType { get; set; }
        void CommitChanges();
        void RejectChanges();
    }
    public interface ITestListItem<T> : ITestListItem 
    {
        string ReferenceValueString { get; }
        string CheckValueString { get; }
        TrackedValue<T> Reference{ get; set; }
        T Check{ get; set; }
        bool Warning { get; }
        string WarningString { get; }
    }
    public interface ITestListClassItem<T> : ITestListItem 
    {
        string ReferenceValueString { get; }
        string CheckValueString { get; }
        TrackedValue<T> Reference { get; set; }
        T Check { get; set; }
        bool Warning { get; }
        string WarningString { get; }
    }
}
