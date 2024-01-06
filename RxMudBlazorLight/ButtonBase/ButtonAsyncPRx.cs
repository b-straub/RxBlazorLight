using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace RxMudBlazorLight.ButtonBase
{
    public class ButtonAsyncPRX<T> : ButtonBaseAsyncRX
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }
        public EventCallback<TouchEventArgs> OnTouch { get; private set; }

        private readonly ICommandAsync<T> _command;

        private ButtonAsyncPRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync<T> command, Action? beforeExecution, Action? afterExecution, string? cancelText, Color? cancelColor) :
            base(type, buttonColor, buttonChildContent, beforeExecution, afterExecution, cancelText, cancelColor, command.CanCancel())
        {
            _command = command;
        }

        public static ButtonAsyncPRX<T> Create(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync<T>? command, Action? beforeExecution, Action? afterExecution, string? cancelText, Color? cancelColor)
        {
            ArgumentNullException.ThrowIfNull(command);
            return new ButtonAsyncPRX<T>(type, buttonColor, buttonChildContent, command, beforeExecution, afterExecution, cancelText, cancelColor);
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameters(T? parameter)
        {
            if (_command.Executing() && _command.CanCancel())
            {
                Color = _cancelColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => _command.Cancel());
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => _command.Cancel());

                Disabled = false;
            }
            else
            {
                Color = _buttonColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = _command.Executing() && _command.HasProgress() ? RenderProgress() : _buttonChildContent;
                }

                _command.SetParameter(parameter);

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => Execute());
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => Execute());

                Disabled = !_command.CanExecute(parameter);
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

        private async Task Execute()
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            await _command.Execute();

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }
}
