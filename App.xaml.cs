using System;

namespace PassManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell())
            {
                Width = 500,  // Set the desired width in pixels
                Height = 700  // Set the desired height in pixels
            };

            // Optional: Lock the size to prevent resizing
            window.MinimumWidth = 500;
            window.MinimumHeight = 700;
            window.MaximumWidth = 500;
            window.MaximumHeight = 700;

            return window;
        }
    }
}