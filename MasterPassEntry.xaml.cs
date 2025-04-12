using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace PassManager
{
    public partial class NewPage1 : ContentPage
    {
        public NewPage1()
        {
            InitializeComponent();
        }
        private readonly string Connect = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=KeyPassZF_DB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

        // Use the same SecureKeyManager as in NewPage2
        private static class SecureKeyManager
        {
            private const string KEY_STORAGE_NAME = "UserEncryptionKey";

            public static async Task<byte[]> GetStoredKeyAsync()
            {
                try
                {
                    string storedKey = await SecureStorage.GetAsync(KEY_STORAGE_NAME);

                    if (string.IsNullOrEmpty(storedKey))
                    {
                        throw new Exception("No encryption key found");
                    }

                    return Convert.FromBase64String(storedKey);
                }
                catch
                {
                    throw new Exception("Unable to retrieve encryption key");
                }
            }
        }

        public static async Task<string> DecryptPasswordAsync(string encryptedText)
        {
            byte[] key = await SecureKeyManager.GetStoredKeyAsync();

            byte[] fullData = Convert.FromBase64String(encryptedText);

            // Extract IV (first 16 bytes) and encrypted data
            byte[] iv = new byte[16];
            byte[] encryptedBytes = new byte[fullData.Length - 16];

            Buffer.BlockCopy(fullData, 0, iv, 0, 16);
            Buffer.BlockCopy(fullData, 16, encryptedBytes, 0, encryptedBytes.Length);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        private async Task SaveUserIdFromEmail(string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Connect))
                {
                    await connection.OpenAsync();

                    string query = "SELECT Id FROM Passwords WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        var result = await command.ExecuteScalarAsync();

                        if (result != null)
                        {
                            int userId = Convert.ToInt32(result);
                            Preferences.Set("LoggedInUserId", userId.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save user ID: {ex.Message}", "OK");
            }
        }

        private async void Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Email.Text) || string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                return;
            }

            string email = Email.Text;
            string enteredPassword = NameEntry.Text;

            try
            {
                string decryptedPassword = await GetDecryptedPassword(email);

                if (decryptedPassword == null)
                {
                    await DisplayAlert("Error", "No password found or decryption failed.", "OK");
                    return;
                }

                if (decryptedPassword == enteredPassword)
                {
                    //  Get and store user ID here after successful login
                    await SaveUserIdFromEmail(email);

                    //  Navigate after storing user ID
                    await Navigation.PushAsync(new ViewAndAddPasswords());
                }
                else
                {
                    await DisplayAlert("Incorrect Password", "Password is incorrect", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<string> GetDecryptedPassword(string email)
        {
            string encryptedData = string.Empty;

            try
            {
                using (SqlConnection connection = new SqlConnection(Connect))
                {
                    await connection.OpenAsync();

                    string query = "SELECT Password FROM Passwords WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                encryptedData = reader.GetString(0);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(encryptedData))
                {
                    return await DecryptPasswordAsync(encryptedData);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to retrieve password: {ex.Message}", "OK");
            }

            return null;
        }



    }
}