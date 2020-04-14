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
        string TestName { get; set; }
        void CommitChanges();
    }
    public interface ITestListItem<T> : ITestListItem
    {
        TestType TestType { get; set; }
        string ReferenceValueString { get; set; }
        string CheckValueString { get; }
        T ReferenceValue { get; set; }
        T CheckValue { get; set; }
        bool Warning { get; }
        string WarningString { get; set; }
        Visibility TestVisibility { get; }
    }
}
