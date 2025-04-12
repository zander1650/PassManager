using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Text;
using System.IO;
using PassManager1;

namespace PassManager;

public partial class ViewAndAddPasswords : ContentPage
{

    public ViewAndAddPasswords()
	{
		InitializeComponent();
	}
    private void ViewPasswords(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ViewPass());
    }
    private void AddPasswords(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AddPass());
    }
    
}
