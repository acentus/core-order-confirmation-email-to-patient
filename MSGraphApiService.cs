using System;
using System.Collections.Generic;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http;
using OrderConfirmationEmailToPatient;

namespace EmailSenderService
{
    public class MSGraphApiService
    {
        private static object _lock = new object();
        private static MSGraphApiService _instance;
        private AppConfig _appConfig;        
        static readonly HttpClient client = new HttpClient();
        private MSGraphApiService(AppConfig appConfig)
        {
            _appConfig = appConfig;                       
        }

        public static MSGraphApiService GetInstance(AppConfig appConfig)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MSGraphApiService(appConfig);
                    }
                }
            }
            return _instance;
        }               

        public async Task SendEmail(string subject, string Body, string fromEmailAddress, string toEmailAddress)
        {
            try
            {
                App.writetoLog("SendEmail start");                                
                var graphServiceClient = GetGraphClient();
                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = Body
                    },
                    ToRecipients = new List<Recipient>()
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = toEmailAddress
                            }
                        }
                    }
                };                
                await graphServiceClient.Users[fromEmailAddress].SendMail(message).Request().PostAsync();
            }
            catch (ServiceException ex)
            {
                App.writetoLog(Newtonsoft.Json.JsonConvert.SerializeObject(ex));                
            }
            catch (Exception ex)
            {
                App.writetoLog(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        private GraphServiceClient GetGraphClient()
        {
            App.writetoLog("GetGraphClient start");
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                // get an access token for Graph
                var accessToken = GetAccessToken();

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken.Result);

                return Task.FromResult(0);
            }));
            App.writetoLog("GetGraphClient end");
            return graphClient;
        }

        private async Task<string> GetAccessToken()
        {
            App.writetoLog("GetAccessToken start");           
            var _httpClient = new HttpClient();
            var url = String.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", _appConfig.TenantId);
            
            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
              { "client_id", _appConfig.    AppId },
              { "grant_type", "client_credentials" },
              { "client_secret", _appConfig.AppSecret},
              { "scope", "https://graph.microsoft.com/.default" }
            });

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(url))
            {
                Content = content
            };

            using (var response = await client.SendAsync(httpRequestMessage))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                Office365TokenResponse myDeserializedClass = System.Text.Json.JsonSerializer.Deserialize<Office365TokenResponse>(responseStream);
                var token = myDeserializedClass.access_token;               
                return token;
            }
        }
    }
}
