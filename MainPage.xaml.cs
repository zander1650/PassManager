﻿namespace PassManager
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new NewPage1());
        }

        private void Fart_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new NewPage2());
        }
    }

}
