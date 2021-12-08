namespace BlueMaria.Services
{
    public interface IRecordingService
    {
        bool CanRecord { get; }
        bool GetCanRecord();
    }
}