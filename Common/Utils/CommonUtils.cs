using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Common.Utils
{
    public class CommonUtils
    {
        public static DateTimeOffset SystemTimeNow => TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);

        public class FixedSaltPasswordHasher<TUser> : PasswordHasher<TUser> where TUser : class
        {
            private readonly byte[] fixedSalt;

            public FixedSaltPasswordHasher(IOptions<PasswordHasherOptions>? optionsAccessor = null) : base(optionsAccessor)
            {
                // Sử dụng một muối cố định
                fixedSalt = Convert.FromBase64String("YmxhYmxhYmxhYmxhYmxhYg=="); // Thay thế bằng muối cố định của bạn
            }

            public override string HashPassword(TUser user, string password)
            {
                ArgumentNullException.ThrowIfNull(password); // Sử dụng ArgumentNullException.ThrowIfNull

                // Sử dụng các phương thức của lớp cơ sở để truy cập _compatibilityMode và _iterCount
                var options = this.GetOptions();
                var compatibilityMode = options.CompatibilityMode;
                var iterCount = options.IterationCount;

                if (compatibilityMode == PasswordHasherCompatibilityMode.IdentityV2)
                {
                    return Convert.ToBase64String(HashPasswordV2(password, fixedSalt));
                }
                else
                {
                    return Convert.ToBase64String(HashPasswordV3(password, fixedSalt, iterCount));
                }
            }

            private static byte[] HashPasswordV2(string password, byte[] salt)
            {
                const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA1; // default for Rfc2898DeriveBytes
                const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
                const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits

                // Produce a version 2 (see comment above) text hash.
                byte[] subkey = KeyDerivation.Pbkdf2(password, salt, Pbkdf2Prf, Pbkdf2IterCount, Pbkdf2SubkeyLength);

                var outputBytes = new byte[1 + salt.Length + Pbkdf2SubkeyLength];
                outputBytes[0] = 0x00; // format marker
                Buffer.BlockCopy(salt, 0, outputBytes, 1, salt.Length);
                Buffer.BlockCopy(subkey, 0, outputBytes, 1 + salt.Length, Pbkdf2SubkeyLength);
                return outputBytes;
            }

            private byte[] HashPasswordV3(string password, byte[] salt, int iterCount)
            {
                return HashPasswordV3(password, salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterCount: iterCount,
                    saltSize: salt.Length,
                    numBytesRequested: 256 / 8);
            }

            private static byte[] HashPasswordV3(string password, byte[] salt, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
            {
                // Produce a version 3 (see comment above) text hash.
                byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

                var outputBytes = new byte[13 + saltSize + subkey.Length];
                outputBytes[0] = 0x01; // format marker
                WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
                WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
                WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
                Buffer.BlockCopy(salt, 0, outputBytes, 13, saltSize);
                Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
                return outputBytes;
            }

            private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
            {
                buffer[offset + 0] = (byte)(value >> 24);
                buffer[offset + 1] = (byte)(value >> 16);
                buffer[offset + 2] = (byte)(value >> 8);
                buffer[offset + 3] = (byte)(value);
            }

            private PasswordHasherOptions GetOptions()
            {
                var options = new PasswordHasherOptions();
                if (typeof(PasswordHasher<TUser>).GetField("_compatibilityMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                {
                    options.CompatibilityMode = (PasswordHasherCompatibilityMode)typeof(PasswordHasher<TUser>)
                        .GetField("_compatibilityMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(this);
                }

                if (typeof(PasswordHasher<TUser>).GetField("_iterCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                {
                    options.IterationCount = (int)typeof(PasswordHasher<TUser>)
                        .GetField("_iterCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(this);
                }

                return options;
            }
        }

        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return "Invalid IP:" + ex.Message;
            }

            return "127.0.0.1";
        }

        public static string GetApiKey(string ApiName)
        {
            try
            {
                // Đường dẫn đến file ApiKey.env trong thư mục IGCSE
                var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ApiKey.env");

                // Kiểm tra file có tồn tại không
                if (!File.Exists(envFilePath))
                {
                    return "Api Key file not found: " + envFilePath;
                }

                // Đọc file và tìm key tương ứng
                var lines = File.ReadAllLines(envFilePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Trim().Equals(ApiName, StringComparison.OrdinalIgnoreCase))
                    {
                        return parts[1].Trim();
                    }
                }

                return "Api Key not found: " + ApiName;
            }
            catch (Exception ex)
            {
                return "Api Key not found:" + ex.Message;
            }
        }

        public static string ObjectToString<T>(T obj)
        {
            if (obj == null) return string.Empty;
            return JsonSerializer.Serialize(obj);
        }

        public static T StringToObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static decimal CosineSimilarity(List<float> v1, List<float> v2)
        {
            decimal dot = 0m;
            decimal mag1 = 0m;
            decimal mag2 = 0m;

            for (int i = 0; i < v1.Count; i++)
            {
                decimal a = (decimal)v1[i];
                decimal b = (decimal)v2[i];
                dot += a * b;
                mag1 += a * a;
                mag2 += b * b;
            }

            decimal denominator = (decimal)(Math.Sqrt((double)mag1) * Math.Sqrt((double)mag2));

            if (denominator == 0)
                return 0m;

            return dot / denominator;
        }

        public static bool isEmtyString(string? str)
        {
            if (str == null)
            {
                return true;
            }
            return string.IsNullOrWhiteSpace(str);
        }
        
        public static bool isEmtyObject(object? obj)
        {
            if (obj == null)
            {
                return true;
            }
            return false;
        }
        
        public static bool isEmtyList<T>(List<T>? list)
        {
            if (list == null || list.Count == 0)
            {
                return true;
            }

            return false;
        }

        public static string GetBase64FromWwwRoot(string webRootPath, string fileName)
        {
            // Xác định thư mục wwwroot (tự tìm trong dự án)
            var filePath = Path.Combine(webRootPath, fileName.TrimStart('/', '\\'));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Không tìm thấy file: {filePath}");
            }

            // Đọc bytes của file
            var fileBytes = File.ReadAllBytes(filePath);

            // Xác định loại MIME dựa theo đuôi file
            var extension = Path.GetExtension(filePath).ToLower();
            string mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            // Trả về chuỗi base64 đầy đủ để gửi qua API
            return $"data:{mimeType};base64,{Convert.ToBase64String(fileBytes)}";
        }
    }
}
