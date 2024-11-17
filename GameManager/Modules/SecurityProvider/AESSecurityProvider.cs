using GameManager.Services;
using Helper;
using System.Security.Cryptography;
using System.Text;

namespace GameManager.Modules.SecurityProvider
{
    public class AESSecurityProvider : ISecurityProvider
    {
        public AESSecurityProvider(IAppPathService appPathService)
        {
            string localKeyPath = Path.Combine(appPathService.ConfigDirPath, "security.key");
            if (Environment.GetEnvironmentVariable("VNGT_SECURITY_KEY") is { } key)
            {
                _key = key;
            }
            else if (File.Exists(localKeyPath))
            {
                _key = File.ReadAllText(localKeyPath);
            }
            else
            {
                _key = Guid.NewGuid().ToString().Substring(0, 16);
                File.WriteAllText(localKeyPath, _key);
            }
        }

        private readonly string _iv = "fiqkngoijbaqjgna";
        private readonly string _key;

        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            await using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                await using (var sw = new StreamWriter(cs))
                {
                    await sw.WriteAsync(plainText);
                }
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public Task<string> DecryptAsync(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return Task.FromResult(string.Empty);
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            string result = "";
            ExceptionHelper.ExecuteWithExceptionHandling(() =>
            {
                using var ms = new MemoryStream(Convert.FromBase64String(encryptedText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                result = sr.ReadToEnd();
            }, _ =>
            {
                result = "";
            });

            return Task.FromResult(result);
        }
    }
}