using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace AzzureFunctionAndMidlware.Middleware
{
    public class Login
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    internal class ValidateUserMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ValidateUserMiddleware> _logger;

        public ValidateUserMiddleware(ILogger<ValidateUserMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            const string username = "username";
            const string password = "password";

            var requestData = await context.GetHttpRequestDataAsync();

            var headers = requestData.Headers;

            var signature = string.Empty;
            var secret = string.Empty;

            if (requestData.Method == "POST")
            {

            }
            
            if (headers.TryGetValues("HeaderName", out var headerValue))
            {
                _logger.LogInformation($"Value of HeaderName header: {headerValue}");

                var arr = headerValue?.FirstOrDefault()?.ToString()?.Split(" ");

                secret = arr[0];
                signature = arr[1];
            }

            // Reads the HTTP request body as a string.
            var body = await new StreamReader(requestData.Body).ReadToEndAsync();

            var dataObject = JObject.Parse(body);

            var login = JsonConvert.SerializeObject(dataObject, Formatting.Indented);

            byte[] hash;
            string hashHex = string.Empty;

            using (HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] jsonData = Encoding.UTF8.GetBytes(login);
                hash = hmac.ComputeHash(jsonData);
                hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            if (!hashHex.Equals(signature))
            {
                var req = await context.GetHttpRequestDataAsync();

                var res = req!.CreateResponse();

                res.StatusCode = HttpStatusCode.BadRequest;

                await res.WriteStringAsync("not valid hash login first");

                context.GetInvocationResult().Value = res;

                return;
            }


            if (dataObject.ContainsKey(username) && dataObject.ContainsKey(password))
            {
                var usr = dataObject?[username].ToString().ToUpper();
                var pwd = dataObject?[password].ToString().ToUpper();

                if (string.IsNullOrWhiteSpace(usr) || string.IsNullOrWhiteSpace( pwd))
                {
                    var req = await context.GetHttpRequestDataAsync();

                    // To set the ResponseData
                    var res = req!.CreateResponse();

                    res.StatusCode = HttpStatusCode.BadRequest;

                    await res.WriteStringAsync("Please login first");

                    context.GetInvocationResult().Value = res;

                    return;
                }


                if (usr != "ADMIN" && pwd != "PASS")
                {
                    var req = await context.GetHttpRequestDataAsync();

                    // To set the ResponseData
                    var res = req!.CreateResponse();
                    
                    res.StatusCode = HttpStatusCode.BadRequest;

                    await res.WriteStringAsync("Please login first");
                    
                    context.GetInvocationResult().Value = res;

                    return;
                }

            }
            else
            {
                var req = await context.GetHttpRequestDataAsync();

                // To set the ResponseData
                var res = req!.CreateResponse();

                res.StatusCode = HttpStatusCode.BadRequest;

                await res.WriteStringAsync("Please login first");

                context.GetInvocationResult().Value = res;

                return;
            }

            requestData.Body.Position = 0;

            _logger.LogInformation("Update context");


            await next(context);

        }
    }
}
