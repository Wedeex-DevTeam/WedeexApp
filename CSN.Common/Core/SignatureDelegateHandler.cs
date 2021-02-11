namespace CSN.Common.Core
{
    using System;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class SignatureDelegateHandler : DelegatingHandler
    {
        #region Constructors

        public SignatureDelegateHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        #endregion Constructors

        #region Methods

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (CoreConfigurationManager.Instance.Configuration == null)
            {
                await CoreConfigurationManager.Instance.InitAsync();
            }
            HttpResponseMessage response = null;
            string requestContentBase64String = string.Empty;

            string requestUri = request.RequestUri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped).ToLowerInvariant();

            string requestHttpMethod = request.Method.Method;

            var requestTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Create random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");

            // Hashing request content
            if (request.Content != null)
            {
                byte[] content = await request.Content.ReadAsByteArrayAsync();
                MD5 md5 = MD5.Create();
                //Hashing the request body, any change in request body will result in different hash
                byte[] requestContentHash = md5.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            // Creating the raw signature string
            string signatureRawData = $"{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentBase64String}";

            var secretKeyByteArray = Convert.FromBase64String(CoreConfigurationManager.Instance.Configuration.SignatureKey);

            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                // Setting the values in the Authorization header using custom scheme (amx)
                request.Headers.Add("Ocp-Apim-Subscription-Key", CoreConfigurationManager.Instance.Configuration.ApiKey);
                request.Headers.Add("x-signature", $"{requestSignatureBase64String}:{nonce}:{requestTimeStamp}:{CoreConfigurationManager.Instance.Configuration.SignatureId}");
            }

            response = await base.SendAsync(request, cancellationToken);

            return response;
        }

        #endregion Methods
    }
}