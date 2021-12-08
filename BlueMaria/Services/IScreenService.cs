namespace BlueMaria.Services
{
    public interface IScreenService
    {
        void ShowNetworkAvailable(bool isAvailable);
        void ShowCanRecord(bool canRecord);
    }
}