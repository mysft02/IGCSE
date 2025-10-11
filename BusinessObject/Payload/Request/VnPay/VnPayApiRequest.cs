using Common.Utils;
using QRCoder;
using System.Globalization;
using System.Net;
using System.Text;

namespace BusinessObject.Payload.Request.VnPay
{
    public class VnPayApiRequest
    {

        public enum ResponseType
        {
            Single,
            Search
        }

        public Dictionary<string, string> Headers { get; set; } = DefaultHeaders();
        public string? BaseUrl { get; set; }
        public object? Body { get; set; }
        public string? Secret { get; set; }
        private readonly SortedList<string, string> QueryParams = new SortedList<string, string>(new VnPayCompare());
        public Dictionary<string, string> PathVariables { get; set; } = new();

        private static Dictionary<string, string> DefaultHeaders()
        {
            return new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };
        }

        public void AddParameter(string key, string value)
        {
            QueryParams[key] = value;
        }

        public void AddPathVariable(string key, string value)
        {
            PathVariables[key] = value;
        }

        public void AddHeader(string key, string value)
        {
            Headers[key] = value;
        }

        public string BuildVnPayUrl()
        {
            var data = new StringBuilder();

            // Loops through the request data and adds each non-empty parameter to the query string.
            foreach (var (key, value) in QueryParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();
            BaseUrl += "?" + querystring;

            // Remove the last '&' from the query string.
            var signData = querystring.TrimEnd('&');

            // Calculate the secure hash using HMACSHA512.
            var vnpSecureHash = CommonUtils.HmacSHA512(Secret, signData);

            // Add the secure hash to the base URL.
            BaseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return BaseUrl;
        }

        public static string ToQrBase64(string text)
        {
            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(20);
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }

        public class VnPayCompare : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var vnpCompare = CompareInfo.GetCompareInfo("en-US");
                return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
            }
        }

        public static VnPayApiRequestBuilder Builder()
        {
            return new VnPayApiRequestBuilder();
        }

        public class VnPayApiRequestBuilder
        {
            private readonly VnPayApiRequest _instance = new VnPayApiRequest();

            public VnPayApiRequestBuilder BaseUrl(string url)
            {
                _instance.BaseUrl = url;
                return this;
            }

            public VnPayApiRequestBuilder Body(object body)
            {
                _instance.Body = body;
                return this;
            }

            public VnPayApiRequestBuilder HashSecret(string secret)
            {
                _instance.Secret = secret;
                return this;
            }

            public VnPayApiRequestBuilder AddParameter(string key, string value)
            {
                _instance.AddParameter(key, value);
                return this;
            }

            public VnPayApiRequestBuilder AddPathVariable(string key, string value)
            {
                _instance.AddPathVariable(key, value);
                return this;
            }

            public VnPayApiRequestBuilder AddHeader(string key, string value)
            {
                _instance.AddHeader(key, value);
                return this;
            }

            public VnPayApiRequest Build()
            {
                _instance.Headers ??= DefaultHeaders();
                _instance.PathVariables ??= new Dictionary<string, string>();
                return _instance;
            }
        }
    }
}
