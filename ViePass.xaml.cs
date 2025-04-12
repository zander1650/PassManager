using Microsoft.Data.SqlClient;

namespace PassManager1;

public partial class ViewPass : ContentPage
{
    private readonly string Connect = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=KeyPassZF_DB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";
    public class StorePassEntry
    {
        public int PasswordId { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
    public async Task<List<StorePassEntry>> GetAllStorePassEntriesAsync()
    {
        List<StorePassEntry> entries = new List<StorePassEntry>();

        try
        {
            using (SqlConnection connection = new SqlConnection(Connect))
            {
                await connection.OpenAsync();

                
                string storedUserId = Preferences.Get("LoggedInUserId", null);

                if (string.IsNullOrEmpty(storedUserId))
                {
                    await DisplayAlert("Error", "User is not logged in.", "OK");
                    return entries;
                }

                int userId = int.Parse(storedUserId);

    
                string query = "SELECT PasswordId, Password, Domain FROM StorePass WHERE PasswordId = @UserId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            entries.Add(new StorePassEntry
                            {
                                PasswordId = reader.GetInt32(0),
                                Password = reader.GetString(1),
                                Domain = reader.GetString(2)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to retrieve data: {ex.Message}", "OK");
        }

        return entries;
    }
    private async void View(object sender, EventArgs e)
    {
        var data = await GetAllStorePassEntriesAsync();
        StorePassCollectionView.ItemsSource = data;
        StorePassCollectionView.IsVisible = true;
    }
    public ViewPass()
    {
        InitializeComponent();
    }
}
