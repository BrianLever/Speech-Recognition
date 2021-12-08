using BlueMaria.StaticFunction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BlueMaria.Services
{
    public class WebAPIService : IWebAPIService
    {
        //private const string _url = "https://localhost:44302/"; // "https://localhost:44363/";
        //private const string _url = "https://192.168.0.101:44302/";
        //private const string _url = "http://192.168.0.101:44301/";

        //private const string _username = "test.dragan.krunic@gmail.com";
        //private const string _password = "testing";
        //private const string _grant_type = "password";

        private const string _url = "https://blue-maria.com/bmapi-nov09th2018-v1/";
        //private const string _username = "hakif@daabox.com";
        //private const string _password = "12345678";
        private const string _grant_type = "password";

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // TODO: instead we should have these...
        public string GetBMToken(string username, string password) => null;
        public string GetGoogleToken(string bmToken) => null;

        public WebAPIService()
        {
            ServicePointManager.ServerCertificateValidationCallback =
            (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
        }

        public async Task<string> GetGoogleTokenAsync(string username, string password)
        {
            //return "ya29.c.ElqKBqtDwdOwE4uR_OumdSe62-8qvgZdWa6gRZeKfX0u-jbJTQ8Wg2zabbAGEbhpSGfeRGAwzyRkHD3DY9s-dn85nwHTNmNMS0-SpU4DJpwA7mR819rL07Nlz-8";
            //return "ya29.c.ElqKBqtDwdOwE4uR_OumdSe62-8qvgZdWa6gRZeKfX0u-jbJTQ8Wg2zabbAGEbhpSGfeRGAwzyRkHD3DY9s-dn85nwHTNmNMS0-SpU4DJpwA7mR819rL07Nlz-8";
            ////return "ya29.c.ElpBBQxD5KtU83oj8C5FS08wIxL85yXvQ036aaF4Bg50C_ymCP4ZLc6Uv4MwqNniw4nput3iuZTcLxMNF4QfzoHOJCGafCc80PVXEpKpyX8K7ofHJGsCuc1Y6eI";
            //return "ya29.c.El8-BQrLCoS2R_zOgZ8bDW8xVQli7BN0OEHL5EsUquX35QbaSHnKed5V01tY2qp6vN8tH5dMTPOSeN6l6LAyV5RYhXKRtyWWeqwp62Mo1GtNX3VYjvE846G185U45neMiQ";
            //return await GetAccessTokenAsync(username, password);
            var bmtoken = await GetAccessTokenAsync(username, password);
            var token = await GetTrackingAsync(bmtoken.accessToken.token);
            return token.googleToken.token;
            //return token;
        }

        public Task<TokenInfo> GetBMTokenAsync(string username, string password) 
            => GetAccessTokenAsync(username, password);

        //public async Task<string> GetBMTokenAsync(string username, string password)
        //{
        //    return await GetAccessTokenAsync(username, password);
        //}

        private static async Task<TokenInfo> GetAccessTokenAsync(string username, string password)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                password = EscapeQuotes(password);

                var parameters = new Dictionary<string, string>
                {
                    ["login"] = username,
                    ["password"] = password,
                };
                var content = new FormUrlEncodedContent(parameters);

                // this is the 'token' endpoint, usually is /token, 
                // but /token is outside of api route a bit odd at times with server setup and relative paths
                //HttpResponseMessage response = await client.PostAsync($"api/token", content); // Token
                if (await InternetCheck.Internet())
                {
                    HttpResponseMessage response = await client.PostAsync($"login", content); // Token

                    if (response.ToString().Contains("StatusCode: 409"))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show($"Google API tokens limit is exceeded" + "\n" + "Please try again later and in the meantime click 'Yes' to help us by reporting this issue to us here.", 
                                                        "Blue-Maria", 
                                                        MessageBoxButton.YesNo, 
                                                        MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                        }
                        return null;
                    }
                    if (response.ToString().Contains("StatusCode: 429"))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show($"Too many requests. Please try again in 60 seconds." + "\n" + "Please try again later and in the meantime click 'Yes' to help us by reporting this issue to us here.", 
                                                            "Blue-Maria", 
                                                            MessageBoxButton.YesNo, 
                                                            MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                        }
                        return null;
                    }
                    if (response.ToString().Contains("StatusCode: 422"))
                    {
                        System.Windows.MessageBox.Show("Sorry, but we do not know these log-in credentials." + "\n" + "\n" + "Please check for typos or mistakes. If you lost your password use the “Reset” button to attain a new one. If you do not have an account yet, please register on our homepage.", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return null;
                    }
                    if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                    {
                        //   System.Windows.MessageBox.Show($"log in failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", "Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);

                        Log.Error($"log in failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        Console.WriteLine($"log in failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        return null;
                    }
                    else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                    {
                        return null;
                    }
                    response.EnsureSuccessStatusCode(); // we should throw as nothing else to do here or above

                    var jsonToken = await response.Content.ReadAsStringAsync();
                    //Token token = JsonConvert.DeserializeObject<Token>(jsonToken);
                    TokenInfo token = JsonConvert.DeserializeObject<TokenInfo>(jsonToken);

                    return token;
                }
                else
                {
                    if (!LocalSettings.StartUp)
                    System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                        "Blue-Maria", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);                    
                    return null;
                }

                // access_token is all we care about and expiration probably (but not all fields are filled up, need to check)
                //return token.access_token;
                //return token.accessToken.token;
            }
        }

        /// <summary>
        /// The single quote and double quote if present needs to be escaped for the content sent to the web API.
        /// </summary>
        /// <param name="password">Input password string</param>
        /// <returns>escaped password</returns>
        private static string EscapeQuotes(string password)
        {
            var pwd = string.Empty;
            pwd = password.Replace(@"'", @"\'");
            pwd = pwd.Replace("\"", "\\\"");
            return pwd;
        }

        public async Task<GoogleTokenInfo> GetTrackingAsync(string token)
        {
           if (LocalSettings.ApiRecordingStatus == "Stop")
            {
                
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_url);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    // this is to test different output content types by specifying accept header explicitly, this works fine (once xml formatters are added)
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var user = new UserInfo { };
                    string json = JsonConvert.SerializeObject(user);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    if (await InternetCheck.Internet())
                    {
                        LocalSettings.ApiRecordingStatus = "Start";
                        HttpResponseMessage response = await client.PostAsync($"tracking/start", content); // Token
                        if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                        {
                            System.Windows.MessageBox.Show($"google start tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", 
                                "Blue-Maria", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                            Log.Error($"google start tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                            Console.WriteLine($"google start tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                            return null;
                        }
                        else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                        {
                            return null;
                        }
                       
                        response.EnsureSuccessStatusCode();

                        var jsonToken = await response.Content.ReadAsStringAsync();
                        GoogleTokenInfo googleToken = JsonConvert.DeserializeObject<GoogleTokenInfo>(jsonToken);
                        return googleToken;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return null;
                    }
                }
            }
            return null;
        }

        public async Task<UserDateInfo> UpdateTrackingStopAsync(string bmtoken)
        {
            if (LocalSettings.ApiRecordingStatus == "Start")
            {
                
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_url);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bmtoken);
                    client.DefaultRequestHeaders
                          .Accept
                          //.Add(new MediaTypeWithQualityHeaderValue("application/xml")); // json"));
                          .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var user = new UserInfo { };
                    string json = JsonConvert.SerializeObject(user);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    if (await InternetCheck.Internet())
                    {
                        LocalSettings.ApiRecordingStatus = "Stop";
                        HttpResponseMessage response = await client.PostAsync($"tracking/stop", content); // Token

                        if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                        {
                            // System.Windows.MessageBox.Show($"google stop tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", "Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);

                            Log.Error($"google stop tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                            Console.WriteLine($"google stop tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                            return null;
                        }
                        else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                        {
                            return null;
                        }

                        response.EnsureSuccessStatusCode();

                        var jsonToken = await response.Content.ReadAsStringAsync();
                        UserDateInfo googleToken = JsonConvert.DeserializeObject<UserDateInfo>(jsonToken);
                        return googleToken;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return null;
                    }
                    // access_token is all we care about and expiration probably (but not all fields are filled up, need to check)
                    //return googleToken.googleToken.token;
                }
            }
            return null;
        }

        public async Task<UserDateInfo> UpdateTrackingPingAsync(string bmtoken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bmtoken);
                client.DefaultRequestHeaders
                      .Accept
                      //.Add(new MediaTypeWithQualityHeaderValue("application/xml")); // json"));
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var user = new UserInfo { };
                string json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                if (await InternetCheck.Internet())
                {
                    HttpResponseMessage response = await client.PostAsync($"tracking/ping", content); // Token

                    if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                    {
                        //  System.Windows.MessageBox.Show($"google ping tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", "Blue-Maria", MessageBoxButton.OK, MessageBoxImage.Error);

                        //  Log.Error($"google ping tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        Console.WriteLine($"google ping tracking failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        return null;
                    }
                    else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                    {
                        return null;
                    }
                    response.EnsureSuccessStatusCode();

                    var jsonToken = await response.Content.ReadAsStringAsync();
                    UserDateInfo googleToken = JsonConvert.DeserializeObject<UserDateInfo>(jsonToken);
                    return googleToken;
                }
                else
                {
                    System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                        "Blue-Maria", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return null;
                }
            }
           
            
        }

        public async Task<GoogleTokenDateInfo> UpdateGoogleTokenRefreshAsync(string bmtoken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bmtoken);
                client.DefaultRequestHeaders
                      .Accept
                      //.Add(new MediaTypeWithQualityHeaderValue("application/xml")); // json"));
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var user = new UserInfo { };
                string json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                if (await InternetCheck.Internet())
                {
                    HttpResponseMessage response = await client.PostAsync($"google-token", content); // Token
                    if (response.ToString().Contains("StatusCode: 409"))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show($"Google API tokens limit is exceeded" + "\n" + "Please try again later and in the meantime click 'Yes' to help us by reporting this issue to us here.", 
                            "Blue-Maria", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                        }
                        return null;
                    }
                    if (response.ToString().Contains("StatusCode: 429"))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show($"Too many requests. Please try again in 60 seconds." + "\n" + "Please try again later and in the meantime click 'Yes' to help us by reporting this issue to us here.", 
                            "Blue-Maria", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
                        }
                        return null;
                    }
                    if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                    {
                        System.Windows.MessageBox.Show($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);

                        Log.Error($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        Console.WriteLine($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        return null;
                    }
                    else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                    {
                        return null;
                    }
                    response.EnsureSuccessStatusCode();

                    var jsonToken = await response.Content.ReadAsStringAsync();
                    GoogleTokenDateInfo googleToken = JsonConvert.DeserializeObject<GoogleTokenDateInfo>(jsonToken);
                    return googleToken;
                }
                else
                {
                    System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                        "Blue-Maria", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return null;
                }
            }
            
        }

        public async Task<BMTokenDateInfo> UpdateBMTokenRefreshAsync(string bmtoken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bmtoken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var user = new UserInfo { };
                string json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                if (await InternetCheck.Internet())
                {
                    HttpResponseMessage response = await client.PostAsync($"access-token", content); // Token
                    if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                    {
                        System.Windows.MessageBox.Show($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        Log.Error($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        Console.WriteLine($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        return null;
                    }
                    else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                    {
                        return null;
                    }
                    response.EnsureSuccessStatusCode();

                    var jsonToken = await response.Content.ReadAsStringAsync();
                    BMTokenDateInfo googleToken = JsonConvert.DeserializeObject<BMTokenDateInfo>(jsonToken);
                    return googleToken;
                }
                else
                {
                    System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                        "Blue-Maria", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }
        //Task<TokenInfo> UpdateBMTokenRefreshAsync(string bmtoken);

        public async Task<BMTokenDateInfo> UpdateBMTokenLogoutAsync(string bmtoken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bmtoken);
                client.DefaultRequestHeaders
                      .Accept
                      //.Add(new MediaTypeWithQualityHeaderValue("application/xml")); // json"));
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var user = new UserInfo { };
                string json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                if (await InternetCheck.Internet())
                {
                    HttpResponseMessage response = await client.PostAsync($"logout", content); // Token

                    if (!response.IsSuccessStatusCode && !response.ToString().Contains("StatusCode: 500"))
                    {
                        System.Windows.MessageBox.Show($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}", 
                            "Blue-Maria", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);

                        Log.Error($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        Console.WriteLine($"google refresh failed: code: {response.StatusCode}, reason: {response.ReasonPhrase}");
                        return null;
                    }
                    else if (!response.IsSuccessStatusCode && response.ToString().Contains("StatusCode: 500"))
                    {
                        return null;
                    }
                    response.EnsureSuccessStatusCode();

                    var jsonToken = await response.Content.ReadAsStringAsync();
                    BMTokenDateInfo googleToken = JsonConvert.DeserializeObject<BMTokenDateInfo>(jsonToken);
                    return googleToken;
                }
                else
                {
                    System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                        "Blue-Maria", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    return null;
                }
            }
        }
    }

    public class Token
    {
        public string userName { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }

    public class TokenInfo
    {
        public string date { get; set; }
        public AccessToken accessToken { get; set; }
        public User user { get; set; }
    }

    public class BMTokenDateInfo
    {
        public string date { get; set; }
        public AccessToken accessToken { get; set; }
        //public User user { get; set; }
    }

    public class UserDateInfo
    {
        public string date { get; set; }
        //public AccessToken accessToken { get; set; }
        public User user { get; set; }
    }

    public class GoogleTokenInfo
    {
        public string date { get; set; }
        public AccessToken googleToken { get; set; }
        public User user { get; set; }
    }

    public class GoogleTokenDateInfo
    {
        public string date { get; set; }
        public AccessToken googleToken { get; set; }
        //public User user { get; set; }
    }

    public class AccessToken
    {
        public string token { get; set; }
        public string tokenType { get; set; }
        public string expiresAt { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public int credits { get; set; }
        //public Credits[] credits { get; set; }
    }

    public class Credits
    {
        public string name { get; set; }
        public string caregory { get; set; }
        public string datestart { get; set; }
        public string dateend { get; set; }
        public string balance { get; set; }
    }

    public class UserInfo
    {
        public string userid { get; set; }
    }

}
