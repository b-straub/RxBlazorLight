using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RxBlazorLightCore;

namespace RxBlazorLight.ButtonBase
{
    public class ButtonPRX<T> : ButtonBaseRX
    {
        public EventCallback<MouseEventArgs> OnClick { get; private set; }

        private readonly ICommand<T> _command;

        private ButtonPRX(MBButtonType type, ICommand<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution) :
            base(type, confirmExecution, beforeExecution, afterExecution)
        {
            ArgumentNullException.ThrowIfNull(command);
            _command = command;
        }

        public static ButtonPRX<T> Create(MBButtonType type, ICommand<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution)
        {
            return new ButtonPRX<T>(type, command, confirmExecution, beforeExecution, afterExecution);
        }

        public void SetParameters(T? parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);

            _command.SetParameter(parameter);
            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => Execute());
        }

        private async Task Execute()
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            bool canExecute = true;

            if (_confirmExecution is not null)
            {
                canExecute = await _confirmExecution();
            }

            if (canExecute)
            {
                _command.Execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }
}
