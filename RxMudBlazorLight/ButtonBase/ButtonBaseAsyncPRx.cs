using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using System.Diagnostics.CodeAnalysis;

namespace RxBlazorLight.ButtonBase
{
    public class ButtonAsyncPRX<T> : ButtonBaseAsyncRX
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }

        private readonly ICommandAsync<T> _command;
        private readonly Color _cancelColor;

        private ButtonAsyncPRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText) :
            base(type, buttonColor, buttonChildContent, confirmExecution, beforeExecution, afterExecution, cancelText)
        {
            ArgumentNullException.ThrowIfNull(command);

            _command = command;
            _cancelColor = cancelColor ?? Color.Warning;
        }

        public static ButtonAsyncPRX<T> Create(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText)
        {
            return new ButtonAsyncPRX<T>(type, buttonColor, buttonChildContent, command, confirmExecution, beforeExecution, afterExecution, cancelColor, cancelText);
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameters(T? parameter)
        {
            if (_command.Executing && _command.CanCancel())
            {
                Color = _cancelColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => _command.Cancel());
                Disabled = false;
            }
            else
            {
                Color = _buttonColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = _command.Executing && _command.HasProgress() ? RenderProgress() : _buttonChildContent;
                }

                _command.Parameter = parameter;

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => Execute());
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

            bool canExecute = true;

            if (_confirmExecution is not null)
            {
                canExecute = await _confirmExecution();
            }

            if (canExecute)
            {
                await _command.Execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }
}
