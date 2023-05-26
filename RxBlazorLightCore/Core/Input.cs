
namespace RxBlazorLightCore
{
    internal class Input<T> : IInput<T>
    {
        public T Value { get; private set; }

        private readonly IRXService _service;
        private readonly Func<bool>? _canChange;

        public Input(IRXService service, T value, Func<bool>? canChange)
        {
            _service = service;
            Value = value;
            _canChange = canChange;
        }

        public Action<T> SetValue => SetValueDo;

        private void SetValueDo(T value) 
        {
            Value = value;
            _service.StateHasChanged();
        }

        public bool CanChange()
        {
            return _canChange is null || _canChange();
        }
    }
}
