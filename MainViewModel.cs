using Prism.Mvvm;

namespace CustomTextBox
{
    internal class MainViewModel : BindableBase
    {
        private float _customValue;
        public float CustomValue
        {
            get => _customValue;
            set => SetProperty(ref _customValue, value, nameof(CustomValue));
        }

        public MainViewModel()
        {
            CustomValue = 123.45f;
        }
    }
}
