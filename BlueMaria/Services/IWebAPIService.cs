using System.Threading.Tasks;

namespace BlueMaria.Services
{
    public interface IWebAPIService
    {
        //string GetBMToken(string username, string password);
        //string GetGoogleToken(string bmToken);
        //TokenInfo GetBMToken(string username, string password);
        Task<TokenInfo> GetBMTokenAsync(string username, string password);
        Task<GoogleTokenInfo> GetTrackingAsync(string bmtoken);
        Task<UserDateInfo> UpdateTrackingStopAsync(string bmtoken);
        Task<UserDateInfo> UpdateTrackingPingAsync(string bmtoken);
        Task<GoogleTokenDateInfo> UpdateGoogleTokenRefreshAsync(string bmtoken);
        Task<BMTokenDateInfo> UpdateBMTokenRefreshAsync(string bmtoken);
        Task<BMTokenDateInfo> UpdateBMTokenLogoutAsync(string bmtoken);
        //Task<string> GetGoogleTokenAsync(string username, string password);
    }
}