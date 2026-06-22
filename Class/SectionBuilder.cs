using ShadowCheat.UILibrary;
using System;
using System.Windows.Controls;

namespace ShadowCheat.Class
{
    public class SectionBuilder
    {
        private readonly StackPanel _panel;
        public SectionBuilder(StackPanel panel) { _panel = panel; }

        public SectionBuilder AddTitle(string title, bool canMinimize, Action<ATitle>? configure = null)
        {
            var c = new ATitle(title, canMinimize);
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }

        public SectionBuilder AddToggle(string title, Action<AToggle>? configure = null, string? tooltip = null)
        {
            var c = new AToggle(title, tooltip);
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }

        public SectionBuilder AddSlider(string title, string label, double freq, double steps, double min, double max, Action<ASlider>? configure = null, string? tooltip = null)
        {
            var c = new ASlider(title, label, steps, tooltip)
            {
                Slider = { Minimum = min, Maximum = max, TickFrequency = freq }
            };
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }

        public SectionBuilder AddSeparator()
        {
            _panel.Children.Add(new ARectangleBottom());
            _panel.Children.Add(new ASpacer());
            return this;
        }

        public SectionBuilder AddColorChanger(string title, Action<AColorChanger>? configure = null)
        {
            var c = new AColorChanger(title);
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }

        public SectionBuilder AddDropdown(string title, Action<ADropdown>? configure = null, string dictionary_path = "")
        {
            var c = new ADropdown(title, dictionary_path);
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }

        public SectionBuilder AddButton(string title, Action<APButton>? configure = null)
        {
            var c = new APButton(title);
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }

        public SectionBuilder AddFileLocator(string title, Action<AFileLocator>? configure = null, string dictionary_path = "")
        {
            var c = new AFileLocator(title, dictionary_path);
            configure?.Invoke(c);
            _panel.Children.Add(c);
            return this;
        }
    }
}
