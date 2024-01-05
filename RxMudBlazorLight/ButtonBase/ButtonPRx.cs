using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RxBlazorLightCore;

namespace RxMudBlazorLight.ButtonBase
{
    public class ButtonPRX<T> : ButtonBaseRX
    {
        public EventCallback<MouseEventArgs> OnClick { get; private set; }
        public EventCallback<TouchEventArgs> OnTouch { get; private set; }

        private readonly ICommand<T> _command;
        private readonly Func<T?, Task<bool>>? _confirmExecution;

        private ButtonPRX(MBButtonType type, ICommand<T>? command, Func<T?, Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution) :
            base(type, beforeExecution, afterExecution)
        {
            ArgumentNullException.ThrowIfNull(command);
            _command = command;
            _confirmExecution = confirmExecution;
        }

        public static ButtonPRX<T> Create(MBButtonType type, ICommand<T>? command, Func<T?, Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution)
        {
            return new ButtonPRX<T>(type, command, confirmExecution, beforeExecution, afterExecution);
        }

        public void SetParameters(T? parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);

            _command.SetParameter(parameter);
            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => Execute());
            OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => Execute());
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
                canExecute = await _confirmExecution(_command.Parameter);
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
