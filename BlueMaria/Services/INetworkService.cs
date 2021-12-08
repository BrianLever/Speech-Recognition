using System.Threading.Tasks;

namespace BlueMaria.Services
{
    public interface INetworkService
    {
        bool IsAvailable { get; set; }
        Task<bool> GetIsAvailable();
    }
}