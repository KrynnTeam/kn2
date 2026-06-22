using System.Windows.Controls;

namespace ShadowCheat.UILibrary
{
    public partial class ASlider : UserControl
    {
        private readonly string _notifierText;
        private Func<double, string>? _valueFormatter;

        public ASlider(string Text, string NotifierText, double ButtonSteps, string? tooltip = null)
        {
            InitializeComponent();
            _notifierText = NotifierText;
            SliderTitle.Content = Text;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = new System.Windows.Controls.ToolTip { Content = tooltip };

            Slider.ValueChanged += (s, e) => UpdateNotifier();
            SubtractOne.Click += (s, e) => Slider.Value = Math.Round(Slider.Value - ButtonSteps, 2);
            AddOne.Click += (s, e) => Slider.Value = Math.Round(Slider.Value + ButtonSteps, 2);
        }

        public void SetValueFormatter(Func<double, string> valueFormatter)
        {
            _valueFormatter = valueFormatter;
            UpdateNotifier();
        }

        private void UpdateNotifier()
        {
            AdjustNotifier.Content = _valueFormatter?.Invoke(Slider.Value) ?? $"{Slider.Value:F2} {_notifierText}";
        }
    }
}
