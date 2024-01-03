using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using System.Diagnostics.CodeAnalysis;

namespace RxMudBlazorLight.ButtonBase
{
    public class ButtonAsyncRX : ButtonBaseAsyncRX
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }

        private readonly ICommandAsync _command;
        private readonly Color _cancelColor;

        private ButtonAsyncRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync? command, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText) :
            base(type, buttonColor, buttonChildContent, beforeExecution, afterExecution, cancelText)
        {
            ArgumentNullException.ThrowIfNull(command);
            _command = command;
            _cancelColor = cancelColor ?? Color.Warning;
        }

        public static ButtonAsyncRX Create(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync? command, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText)
        {
            return new ButtonAsyncRX(type, buttonColor, buttonChildContent, command, beforeExecution, afterExecution, cancelColor, cancelText);
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameters()
        {
            Disabled = !_command.CanExecute() && !(_command.Executing && _command.CanCancel());

            if (_command.Executing && _command.CanCancel())
            {
                Color = _cancelColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => _command.Cancel());
            }
            else
            {
                Color = _buttonColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = _command.Executing && _command.HasProgress() ? RenderProgress() : _buttonChildContent;
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => Execute(_command.Execute));
            }
        }

        public (string? StartIcon, string? EndIcon, string? Label) GetFabParameters(string? startIcon, string? endIcon, string? label, MBIconVariant? iconVariant = null)
        {
            return GetFabParametersBase(_command, startIcon, endIcon, label, iconVariant);
        }

        public string GetIconButtonParameters(string icon, MBIconVariant? iconVariant = null)
        {
            return GetIconButtonParametersBase(_command, icon, iconVariant);
        }

        private async Task Execute(Func<Task> execute)
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            await execute();

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }
}
