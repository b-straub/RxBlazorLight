using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RxBlazorLightCore;

namespace RxBlazorLight.ButtonBase
{
    public class ButtonRX : ButtonBaseRX
    {
        public EventCallback<MouseEventArgs> OnClick { get; }

        private ButtonRX(MBButtonType type, ICommand? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution) :
            base(type, confirmExecution, beforeExecution, afterExecution)
        {
            ArgumentNullException.ThrowIfNull(command);
            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, async () => await Execute(command.Execute));
        }

        public static ButtonRX Create(MBButtonType type, ICommand? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution)
        {
            return new ButtonRX(type, command, confirmExecution, beforeExecution, afterExecution);
        }

        private async Task Execute(Action execute)
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
                execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }
}
