using System;
using System.Threading.Tasks;

namespace Brain2CPU.MvvmEssence
{
    public delegate Task ActionAsync();
    public delegate Task ActionAsync<in T>(T obj);


    public class RelayCommandAsync : RelayCommandBase
    {
        private readonly ActionAsync _execute;

        public RelayCommandAsync(ActionAsync execute, Func<bool> canExecute = null) : base(canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override async void Execute(object parameter) => await _execute();

        public async void Execute() => await _execute();
    }

    public class RelayCommandAsync<T> : RelayCommandBase
    {
        private readonly ActionAsync<T> _execute;

        public RelayCommandAsync(ActionAsync<T> execute, Func<bool> canExecute = null) : base(canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override async void Execute(object parameter) => await _execute((T)parameter);
    }
}