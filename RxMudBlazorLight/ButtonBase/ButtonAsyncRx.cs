using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace RxMudBlazorLight.ButtonBase
{
    public class ButtonAsyncRX : ButtonBaseAsyncRX
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }
        public EventCallback<TouchEventArgs> OnTouch { get; private set; }

        private readonly ICommandAsync _command;

        private ButtonAsyncRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync command, Action? beforeExecution, Action? afterExecution, string? cancelText, Color? cancelColor) :
            base(type, buttonColor, buttonChildContent, beforeExecution, afterExecution, cancelText, cancelColor, command.CanCancel())
        {
            _command = command;
        }

        public static ButtonAsyncRX Create(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync? command, Action? beforeExecution, Action? afterExecution, string? cancelText, Color? cancelColor)
        {
            ArgumentNullException.ThrowIfNull(command);
            return new ButtonAsyncRX(type, buttonColor, buttonChildContent, command, beforeExecution, afterExecution, cancelText, cancelColor);
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]

        public void SetParameters()
        {
            Disabled = !_command.CanExecute() && !(_command.Executing() && _command.CanCancel());

            if (_command.Executing() && _command.CanCancel())
            {
                Color = _cancelColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => _command.Cancel());
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => _command.Cancel());
            }
            else
            {
                Color = _buttonColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = _command.Executing() && _command.HasProgress() ? RenderProgress() : _buttonChildContent;
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => Execute(_command.Execute));
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => Execute(_command.Execute));
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
