using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BlueMaria.StaticFunction
{
    public class InternetCheck
    {
        public static async Task<bool> Internet()
        {
            try
            {
                using (var client = new WebClient())
                using (await client.OpenReadTaskAsync("https://www.google.com"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> GetIsAvailable()
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                        webClient.Proxy = null;
                        using (var stream = await webClient.OpenReadTaskAsync(new Uri("http://www.google.com")))
                        {
                            return true;
                        }
                        //using (var stream = await webClient.OpenReadTaskAsync(new Uri("http://www.google.com")))
                        //{
                        //    return stream != null;
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                // System.Net.WebException
            }
            return false;
        }
    }
}
