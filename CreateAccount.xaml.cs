using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Text;
using System.IO;

namespace PassManager
{
    public partial class NewPage2 : ContentPage
    {
        // Hardcoded Key (256-bit AES key, Base64 encoded)
        private readonly string Connect = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=KeyPassZF_DB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

        public static readonly byte[] EncryptionKey = Convert.FromBase64String("dGhpcy1pcy1hLXRlc3Qtc3RyaW5nLWZvci1hZXMtMjU2LWJpdA=A");

        public NewPage2()
        {
            InitializeComponent();
        }
        public static class SecureKeyManager
        {
            private const string KEY_STORAGE_NAME = "UserEncryptionKey";

            // Generate a new encryption key
            public static byte[] GenerateAndStoreKey()
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.GenerateKey();

                    // Convert key to base64 for secure storage
                    string base64Key = Convert.ToBase64String(aesAlg.Key);

                    // Store the key securely
                    SecureStorage.SetAsync(KEY_STORAGE_NAME, base64Key);

                    return aesAlg.Key;
                }
            }

            // Retrieve the stored encryption key
            public static async Task<byte[]> GetStoredKeyAsync()
            {
                try
                {
                    string storedKey = await SecureStorage.GetAsync(KEY_STORAGE_NAME);

                    if (string.IsNullOrEmpty(storedKey))
                    {
                        // If no key exists, generate a new one
                        return GenerateAndStoreKey();
                    }

                    return Convert.FromBase64String(storedKey);
                }
                catch
                {
                    // If retrieval fails, generate a new key
                    return GenerateAndStoreKey();
                }
            }

            // Encryption method
            public static async Task<string> EncryptPasswordAsync(string plainText)
            {
                byte[] key = await GetStoredKeyAsync();

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.GenerateIV();

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }

                        byte[] encryptedBytes = msEncrypt.ToArray();

                        // Combine IV and encrypted data
                        byte[] result = new byte[aesAlg.IV.Length + encryptedBytes.Length];
                        Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
                        Buffer.BlockCopy(encryptedBytes, 0, result, aesAlg.IV.Length, encryptedBytes.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }
            private async void Create2(object sender, EventArgs e)
        {
            // Clear previous errors
            FnameError.IsVisible = false;
            LnameError.IsVisible = false;
            EmailError.IsVisible = false;
            MasterError.IsVisible = false;

            // Validate input fields
            if (string.IsNullOrWhiteSpace(Fname.Text))
            {
                FnameError.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(Lname.Text))
            {
                LnameError.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(Email.Text) ||
                !(Email.Text.Contains('@') && (Email.Text.ToLower().Contains(".com") || Email.Text.ToLower().Contains(".ca"))))
            {
                EmailError.IsVisible = true;
                await DisplayAlert("Invalid Email", "Email is invalid or has missing elements", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(Mpassword.Text))
            {
                MasterError.IsVisible = true;
                return;
            }

            string firstName = Fname.Text;
            string lastName = Lname.Text;
            string email = Email.Text;
            string masterPassword = Mpassword.Text;
            string masterPassword2 = Mpassword2.Text;

            if (masterPassword != masterPassword2)
            {
                await DisplayAlert("Error", "Passwords Do Not Match", "Exit");
                return;
            }

            User user = new User(firstName, lastName, email, masterPassword);
            await DisplayAlert("Account Created", user.ToString(), "Exit");

            try
            {
                string encryptedData =  await SecureKeyManager.EncryptPasswordAsync(masterPassword);

                // Store the encrypted password in the database
                await AddPasswordToTable(firstName, lastName, email, encryptedData);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Encryption or Database Error: {ex.Message}", "OK");
            }
        }

        // ✅ Encrypt password with consistent key
       

        private async Task AddPasswordToTable(string firstName, string lastName, string email, string password)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(Connect))
                {
                    await connection.OpenAsync();

                    string query = "INSERT INTO Passwords (FirstName, LastName, Email, Password) VALUES (@FirstName, @LastName, @Email, @Password)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                await DisplayAlert("Success", "Password added successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to add password: {ex.Message}", "OK");
            }
        }

        public class User
        {
            public string Fname { get; set; }
            public string Lname { get; set; }
            public string Email { get; set; }
            public string Mpassword { get; set; }

            public User(string fname, string lname, string email, string mpassword)
            {
                Fname = fname;
                Lname = lname;
                Email = email;
                Mpassword = mpassword;
            }

            public override string ToString()
            {
                return $"Name: {Fname} {Lname}, Email: {Email}, MasterPassword: {Mpassword}";
            }
        }
    }
}
