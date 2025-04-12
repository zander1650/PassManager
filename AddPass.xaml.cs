using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace PassManager;

public partial class AddPass : ContentPage
{
    private readonly string Connect = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=KeyPassZF_DB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

    public AddPass()
    {
        InitializeComponent();
    }

    // Store Password as object (Domain, Password)
    public class StoredPasswords
    {
        public string Domain { get; set; }
        public string Password { get; set; }

        public StoredPasswords(string domain, string password)
        {
            Domain = domain;
            Password = password;
        }
    }

  
private async Task AddPasswordToTable(string password, string domain)
{
    try
    {
        using (SqlConnection connection = new SqlConnection(Connect))
        {
            await connection.OpenAsync();

 

                string passwordId = Preferences.Get("LoggedInUserId", null);


                // 2. Insert into StorePass using the retrieved PasswordId
                string insertQuery = @"
        INSERT INTO StorePass (PasswordId, Password, Domain)
        VALUES (@PasswordId, @Password, @Domain);";

            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@PasswordId", passwordId);
                insertCommand.Parameters.AddWithValue("@Password", password);
                insertCommand.Parameters.AddWithValue("@Domain", domain);

                int rowsAffected = await insertCommand.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Success", "Password added successfully!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to add password.", "OK");
                }
            }
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", $"Failed to add password: {ex.Message}", "OK");
    }
}

    private void Submit2(object sender, EventArgs e)
    {
        string pass = Password.Text;
        string domain = DomainOrWebsite.Text;

        if (!string.IsNullOrWhiteSpace(pass) && !string.IsNullOrWhiteSpace(domain))
        {
            _ = AddPasswordToTable(pass, domain);
            Password.Text = string.Empty;
            DomainOrWebsite.Text = string.Empty;
        }
        else
        {
            DisplayAlert("Error", "All fields are required.", "OK");
        }
    }
}
