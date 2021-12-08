using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.Net.Cache;
using BlueMaria.StaticFunction;

namespace BlueMaria.Services
{
    public class NetworkService : INetworkService
    {
        //IObservable<bool>;
        private readonly IScreenService _screenService;

        //private bool? _isAvailable = null;
        //public bool IsAvailable { get { return _isAvailable } set { } }
        public bool IsAvailable { get; set; }

        public NetworkService(IScreenService screenService)
        {
            _screenService = screenService;

            NetworkChange.NetworkAddressChanged += async (s,e) => await OnAvailabilityChanged(null); // NetworkChange_NetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged += async (s, e) => await OnAvailabilityChanged(e.IsAvailable); // NetworkChange_NetworkAvailabilityChanged;
        }

        //public void RefreshAvailability()
        //{
        //    IsAvailable = await GetIsAvailable();
        //    _screenService.ShowNetworkAvailable(IsAvailable);
        //}

        private async Task OnAvailabilityChanged(bool? isAvailable)
        {
            
            IsAvailable = await GetIsAvailable();
            LocalSettings.IsNetAvailable = IsAvailable;
            _screenService.ShowNetworkAvailable(IsAvailable);
        }

        public async Task<bool> GetIsAvailable()
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

            //try
            //{
            //    using (TcpClient client = new TcpClient("www.google.com", 80))
            //    {
            //        client.Close(); // not needed probably, recheck
            //    }
            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}

            //try
            //{
            //    using (var client = new HttpClient())
            //    {
            //        client.BaseAddress = new Uri("");
            //        client.
            //    }
            //}
            //catch (Exception)
            //{
            //}
        }
        //private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        //{
        //    OnAvailabilityChanged(e.IsAvailable);
        //}

        //private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        //{
        //    OnAvailabilityChanged(null);
        //}
    }
}
