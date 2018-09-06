using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Selemation.DriverExtensions
{
    /// <summary>
    /// From internet to allow connect to protected by basic auth selenium server
    /// </summary>
    public class AuthorizedHttpCommandExecutor : ICommandExecutor
    {
        protected const string JsonMimeType = "application/json";
        protected const string PngMimeType = "image/png";
        protected const string CharsetType = "charset=utf-8";
        protected const string ContentTypeHeader = JsonMimeType + ";" + CharsetType;
        protected const string RequestAcceptHeader = JsonMimeType + ", " + PngMimeType;

        protected Uri RemoteServerUri { get; set; }

        protected TimeSpan ServerResponseTimeout { get; set; }

        protected bool EnableKeepAlive { get; set; }

        protected bool UseBasicAuth { get; set; }

        public CommandInfoRepository CommandInfoRepository { get; set; } = new WebDriverWireProtocolCommandInfoRepository();

        public AuthorizedHttpCommandExecutor(Uri addressOfRemoteServer, TimeSpan timeout, bool keepAlive = true, bool useBasicAuth = false)
        {
            if (addressOfRemoteServer == null)
            {
                throw new ArgumentNullException("addressOfRemoteServer", "You must specify a remote address to connect to");
            }

            if (!addressOfRemoteServer.AbsoluteUri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                addressOfRemoteServer = new Uri(addressOfRemoteServer + "/");
            }

            RemoteServerUri = addressOfRemoteServer;
            ServerResponseTimeout = timeout;

            EnableKeepAlive = keepAlive;
            UseBasicAuth = useBasicAuth;

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 2000;

            if (Type.GetType("Mono.Runtime", false, true) == null)
            {
                HttpWebRequest.DefaultMaximumErrorResponseLength = -1;
            }
        }

        protected static string GetTextOfWebResponse(HttpWebResponse webResponse)
        {
            // StreamReader.Close also closes the underlying stream.
            Stream responseStream = webResponse.GetResponseStream();
            StreamReader responseStreamReader = new StreamReader(responseStream, Encoding.UTF8);
            string responseString = responseStreamReader.ReadToEnd();
            responseStreamReader.Close();

            // The response string from the Java remote server has trailing null
            // characters. This is due to the fix for issue 288.
            if (responseString.IndexOf('\0') >= 0)
            {
                responseString = responseString.Substring(0, responseString.IndexOf('\0'));
            }

            return responseString;
        }

        public Response Execute(Command commandToExecute)
        {
            if (commandToExecute == null)
                throw new ArgumentNullException(nameof(commandToExecute), "commandToExecute cannot be null");

            var info = CommandInfoRepository.GetCommandInfo(commandToExecute.Name);
            var requestInfo = new HttpRequestInfo(RemoteServerUri, commandToExecute, info);

            HttpResponseInfo responseInfo = MakeHttpRequest(requestInfo);

            Response toReturn = CreateResponse(responseInfo);
            if (commandToExecute.Name == DriverCommand.NewSession && toReturn.IsSpecificationCompliant)
            {
                // If we are creating a new session, sniff the response to determine
                // what protocol level we are using. If the response contains a
                // field called "status", it's not a spec-compliant response.
                // Each response is polled for this, and sets a property describing
                // whether it's using the W3C protocol dialect.
                // TODO(jimevans): Reverse this test to make it the default path when
                // most remote ends speak W3C, then remove it entirely when legacy
                // protocol is phased out.
                CommandInfoRepository = new W3CWireProtocolCommandInfoRepository();
            }

            return toReturn;
        }

        protected HttpResponseInfo MakeHttpRequest(HttpRequestInfo requestInfo)
        {
            var request = CreateHttpWebRequest(requestInfo);

            HttpResponseInfo responseInfo = new HttpResponseInfo();
            HttpWebResponse webResponse = null;
            try
            {
                webResponse = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                webResponse = ex.Response as HttpWebResponse;
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    string timeoutMessage = "The HTTP request to the remote WebDriver server for URL {0} timed out after {1} seconds.";
                    throw new WebDriverException(string.Format(CultureInfo.InvariantCulture, timeoutMessage, request.RequestUri.AbsoluteUri, ServerResponseTimeout.TotalSeconds), ex);
                }
                else if (ex.Response == null)
                {
                    string nullResponseMessage = "A exception with a null response was thrown sending an HTTP request to the remote WebDriver server for URL {0}. The status of the exception was {1}, and the message was: {2}";
                    throw new WebDriverException(string.Format(CultureInfo.InvariantCulture, nullResponseMessage, request.RequestUri.AbsoluteUri, ex.Status, ex.Message), ex);
                }
            }

            if (webResponse == null)
            {
                throw new WebDriverException("No response from server for url " + request.RequestUri.AbsoluteUri);
            }
            else
            {
                responseInfo.Body = GetTextOfWebResponse(webResponse);
                responseInfo.ContentType = webResponse.ContentType;
                responseInfo.StatusCode = webResponse.StatusCode;
            }

            return responseInfo;
        }

        protected Response CreateResponse(HttpResponseInfo stuff)
        {
            Response commandResponse = new Response();
            string responseString = stuff.Body;
            if (stuff.ContentType != null && stuff.ContentType.StartsWith(JsonMimeType, StringComparison.OrdinalIgnoreCase))
            {
                commandResponse = Response.FromJson(responseString);
            }
            else
            {
                commandResponse.Value = responseString;
            }

            if (CommandInfoRepository.SpecificationLevel < 1 && (stuff.StatusCode < HttpStatusCode.OK || stuff.StatusCode >= HttpStatusCode.BadRequest))
            {
                // 4xx represents an unknown command or a bad request.
                if (stuff.StatusCode >= HttpStatusCode.BadRequest && stuff.StatusCode < HttpStatusCode.InternalServerError)
                {
                    commandResponse.Status = WebDriverResult.UnhandledError;
                }
                else if (stuff.StatusCode >= HttpStatusCode.InternalServerError)
                {
                    // 5xx represents an internal server error. The response status should already be set, but
                    // if not, set it to a general error code. The exception is a 501 (NotImplemented) response,
                    // which indicates that the command hasn't been implemented on the server.
                    if (stuff.StatusCode == HttpStatusCode.NotImplemented)
                    {
                        commandResponse.Status = WebDriverResult.UnknownCommand;
                    }
                    else
                    {
                        if (commandResponse.Status == WebDriverResult.Success)
                        {
                            commandResponse.Status = WebDriverResult.UnhandledError;
                        }
                    }
                }
                else
                {
                    commandResponse.Status = WebDriverResult.UnhandledError;
                }
            }

            if (commandResponse.Value is string)
            {
                // First, collapse all \r\n pairs to \n, then replace all \n with
                // System.Environment.NewLine. This ensures the consistency of
                // the values.
                commandResponse.Value = ((string)commandResponse.Value).Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            }

            return commandResponse;
        }
        protected virtual HttpWebRequest CreateHttpWebRequest(HttpRequestInfo requestInfo)
        {
            HttpWebRequest request = WebRequest.Create(requestInfo.FullUri) as HttpWebRequest;
            request.Method = requestInfo.HttpMethod;
            request.Timeout = (int)ServerResponseTimeout.TotalMilliseconds;
            request.Accept = RequestAcceptHeader;
            request.KeepAlive = EnableKeepAlive;
            request.ServicePoint.ConnectionLimit = 2000;

            if (request.Method == CommandInfo.PostCommand)
            {
                string payload = requestInfo.RequestBody;
                byte[] data = Encoding.UTF8.GetBytes(payload);
                request.ContentType = ContentTypeHeader;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }

            if (!string.IsNullOrEmpty(requestInfo.FullUri.UserInfo) && UseBasicAuth)
            {
                string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(requestInfo.FullUri.UserInfo));
                request.Headers.Add(HttpRequestHeader.Authorization, $"Basic {basicAuth}");
            }

            return request;
        }

        protected class HttpRequestInfo
        {
            public HttpRequestInfo(Uri serverUri, Command commandToExecute, CommandInfo commandInfo)
            {
                FullUri = commandInfo.CreateCommandUri(serverUri, commandToExecute);
                HttpMethod = commandInfo.Method;
                RequestBody = commandToExecute.ParametersAsJsonString;
            }

            public Uri FullUri { get; set; }
            public string HttpMethod { get; set; }
            public string RequestBody { get; set; }
        }

        protected class HttpResponseInfo
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Body { get; set; }
            public string ContentType { get; set; }
        }

        public void Dispose()
        {
        }
    }
}
