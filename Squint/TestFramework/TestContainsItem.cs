using PropertyChanged;
using SquintScript.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using SquintScript.Extensions;
using SquintScript.ViewModels;

namespace SquintScript.TestFramework
{
    [AddINotifyPropertyChangedInterface]
    public class TestContainsItem<T> : TestListItem<T>, ITestListItem<T>
    {
        public EditTypes EditType { get; private set; } = EditTypes.AnyOfValues;

        public bool ItemHasDisplayName { get; private set; } = false;
        public void SetCheckValue(object CheckThis)
        {
            Check = (T)CheckThis;
            if (Check is IContains<T>)
            {
                foreach (var S in ReferenceCollection)
                {
                    ((IContains<T>)Check).Contains(S);
                }
            }
            RaisePropertyChangedEvent(nameof(CheckPass));
            RaisePropertyChangedEvent(nameof(CheckValueString));
            RaisePropertyChangedEvent(nameof(Warning));
            RaisePropertyChangedEvent(nameof(WarningString));
        }
        public ObservableCollection<T> ReferenceCollection { get; set; }
        public ObservableCollection<string> ReferenceCollectionString { get; set; } = new ObservableCollection<string>();
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceCollection == null)
                    return _EmptyRefValueString;
                else
                {
                    if (ReferenceCollection.FirstOrDefault() is IDisplayable)
                    {
                        return string.Format("{0}", string.Join("\r\n", ReferenceCollection.Select(x => (x as IDisplayable).DisplayName)));
                    }
                    if (Check is Enum)
                        return string.Format("{0}", string.Join(", ", ReferenceCollection.Select(x => (x as Enum).Display())));
                    else
                        return string.Format("{0}", string.Join(", ", ReferenceCollection));
                }
            }
        }
        public bool IsDirty { get { return false; } } // at present no way to manually change ReferenceCollection
        public string CheckValueString
        {
            get
            {
                if (Check == null)
                    return _EmptyCheckValueString;
                else
                {
                    if (Check is double || Check is int)
                        return string.Format("{0:0.###}", Check);
                    else if (Check is Enum)
                        return (Check as Enum).Display();
                    else if (Check is IDisplayable)
                        return ((IDisplayable)Check).DisplayName;
                    else
                        return Check.ToString();
                }
            }
        }

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
        public bool? Warning
        {
            get
            {
                if (CheckPass == null)
                    return null;
                else
                    return !(bool)CheckPass;

            }
            set { }
        }
        public bool? CheckPass
        {
            get
            {
                if (ReferenceCollection == null)
                    if (ParameterOption == ParameterOptions.Required)
                        return false;
                    else
                        return null;
                else if (Check == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return false;
                    else
                        return null;
                }
                else
                {
                    if (ReferenceCollection.Contains((T)Check))
                        return true;
                    else
                        return false;
                }
            }
        }
        public ObservableCollection<T> EnumOptions { get; set; }

        public T SetReference { get; set; }
        public TestContainsItem(CheckTypes CT, T V, ObservableCollection<T> referenceCollection, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            CheckType = CT;
            ReferenceCollection = referenceCollection;
            if (V is Enum)
            {
                EnumOptions = new ObservableCollection<T>();
                foreach (T val in Enum.GetValues(typeof(T)))
                {
                    EnumOptions.Add(val);
                }
                SetReference = EnumOptions.FirstOrDefault();
            }
            else
            {
                SetReference = referenceCollection.FirstOrDefault();
            }
            Check = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
            if (Check is IDisplayable)
                ItemHasDisplayName = true;
        }
        public System.Windows.Input.ICommand RemoveItemCommand
        {
            get { return new DelegateCommand(RemoveItem); }
        }
        public void RemoveItem(object param = null)
        {
            if (param != null)
            {
                ReferenceCollection.Remove((T)param);
                RaisePropertyChangedEvent(nameof(ReferenceCollection));
                RaisePropertyChangedEvent(nameof(ReferenceValueString));
            }
        }
        public System.Windows.Input.ICommand AddItemCommand
        {
            get { return new DelegateCommand(AddItem); }
        }
        public void AddItem(object param = null)
        {
            if (param != null)
            {
                ReferenceCollection.Add((T)param);
                RaisePropertyChangedEvent(nameof(ReferenceCollection));
                RaisePropertyChangedEvent(nameof(ReferenceValueString));
            }
        }
        public void CommitChanges() { }
        public void RejectChanges()
        {
            if (Reference != null)
            {
                Reference.RejectChanges();
            }
        }
    }

}


