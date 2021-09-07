using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

// if validation fails the value is still saved and the property name is added to the InvalidFields set

namespace Brain2CPU.MvvmEssence
{
    public delegate bool IsValid(object o);

    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //no backing field needed
        private readonly Dictionary<string, object> _fieldValues = new();
        
        private readonly Dictionary<string, IsValid> _fieldValidators = new();

        public HashSet<string> InvalidFields { get; } = new();

        public bool IsObjectValid => InvalidFields.Count == 0;

        protected T Get<T>(IsValid validator = null, [CallerMemberName] string propertyName = null) => Get(default(T), validator, propertyName);

        protected T Get<T>(T defaultVal, IsValid validator = null, [CallerMemberName] string propertyName = null)
        {
            if(_fieldValues.TryGetValue(propertyName, out object v))
                return (T)v;

            _fieldValues.Add(propertyName, defaultVal);

            if (validator != null)
            {
                _fieldValidators.Add(propertyName, validator);

                Validate(defaultVal, propertyName);
            }

            return defaultVal;
        }

        protected delegate bool EqualityChecker<T>(T t1, T t2);

        private void Validate(object value, string propertyName)
        {
            if (!_fieldValidators.TryGetValue(propertyName, out IsValid validator) || validator == null) 
                return;

            bool b;
            try
            {
                b = validator(value);
            }
            catch (Exception xcp)
            {
                b = false;
                Debug.WriteLine(xcp);
            }

            if (b)
                InvalidFields.Remove(propertyName);
            else
                InvalidFields.Add(propertyName);
        }

        protected bool Set<T>(T value, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null)
        {
            Validate(value, propertyName);

            if (_fieldValues.TryGetValue(propertyName, out object v))
            {
                if (equalityChecker?.Invoke((T)v, value) ?? EqualityComparer<T>.Default.Equals((T)v, value))
                    return false;

                _fieldValues[propertyName] = value;
            }
            else
                _fieldValues.Add(propertyName, value);

            NotifyPropertyChanged(propertyName);

            return true;
        }

        //setter for backing field
        protected bool Set<T>(ref T storage, T value, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null)
        {
            Validate(value, propertyName);

            if (equalityChecker?.Invoke(storage, value) ?? EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;

            NotifyPropertyChanged(propertyName);

            return true;
        }

        // executes get for all public not-command properties
        protected void InitializeObject()
        {
            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // this avoid Get only properties, if better filtering is needed must define an attribute
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                    propertyInfo.GetValue(this);
            }
        }
    }
}
