using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Brain2CPU.MvvmEssence
{
    public class ViewModelBase : ObservableObject
    {
        public event Action OnInitialized;

        // StartInitialization is called before the child class constructor is even started !!!
        // the auto-initialized constructor can be used only if the child constructor is empty or 
        // whatever initialization is done in it will not affect the async initialization
        protected ViewModelBase() : this(true)
        {
        }

        protected ViewModelBase(bool initialize)
        {
            if (initialize)
                StartInitialization();
        }

        //no backing field needed
        protected bool Set<T>(T value, RelayCommandBase cmd, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null) =>
            Set(value, new[] {cmd}, equalityChecker, propertyName);

        protected bool Set<T>(T value, IEnumerable<RelayCommandBase> cmds, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null)
        {
            var b = Set(value, equalityChecker, propertyName);

            if(b)
            {
                foreach(var cmd in cmds)
                {
                    cmd.RaiseCanExecuteChanged();
                }
            }

            return b;
        }

        //setter for backing field
        protected bool Set<T>(ref T storage, T value, RelayCommandBase cmd, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null) =>
            Set(ref storage, value, new[] { cmd }, equalityChecker, propertyName);

        protected bool Set<T>(ref T storage, T value, IEnumerable<RelayCommandBase> cmds, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null)
        {
            var b = Set(ref storage, value, equalityChecker, propertyName);

            if(b)
            {
                foreach(var cmd in cmds)
                {
                    cmd.RaiseCanExecuteChanged();
                }
            }

            return b;
        }

        //command getters
        private readonly Dictionary<string, RelayCommandBase> _commands = new Dictionary<string, RelayCommandBase>();

        protected RelayCommand Get(Action execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
        {
            if(_commands.TryGetValue(commandName, out RelayCommandBase c))
                return (RelayCommand)c;

            var nc = new RelayCommand(execute, canExecute);
            _commands.Add(commandName, nc);
            return nc;
        }

        protected RelayCommand<T> Get<T>(Action<T> execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
        {
            if(_commands.TryGetValue(commandName, out RelayCommandBase c))
                return (RelayCommand<T>)c;

            var nc = new RelayCommand<T>(execute, canExecute);
            _commands.Add(commandName, nc);
            return nc;
        }


        //stock busy flag
        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                NotifyPropertyChanged();

                foreach (var cmd in _commands.Values)
                {
                    cmd.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isInitialized = false;
        public bool IsInitialized
        {
            get => _isInitialized;
            private set
            {
                if (_isInitialized == value)
                    return;

                _isInitialized = value;
                NotifyPropertyChanged();

                foreach (var cmd in _commands.Values)
                {
                    cmd.RaiseCanExecuteChanged();
                }
            }
        }

        protected async void StartInitialization()
        {
            await InitializeAsync();
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        protected virtual Task InitializeAsync() => Task.FromResult(false);
    }
}
