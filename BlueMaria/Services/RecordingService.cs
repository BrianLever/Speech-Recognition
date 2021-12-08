using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Windows;

namespace BlueMaria.Services
{
    public class RecordingService : IRecordingService, IMMNotificationClient
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IScreenService _screenService;
        private readonly MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();

        public bool CanRecord { get; set; }

        public RecordingService(IScreenService screenService)
        {
            _screenService = screenService;
            //OnAvailabilityChanged(null);

            _deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        public void Stop()
        {
            try
            {
                _deviceEnumerator.UnregisterEndpointNotificationCallback(this);
            }
            catch (Exception e)
            {
                Log.Error($"recording Stop failed...", e);
            }
            finally
            {
                try
                {
                    _deviceEnumerator.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"recording Stop failed (2)...", e);
                }
            }
        }

        private void OnAvailabilityChanged(bool? isAvailable)
        {
            CanRecord = GetCanRecord();
            _screenService.ShowCanRecord(CanRecord);
        }

        public bool GetCanRecord()
        {
            try
            {
                var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                return devices.Any();
                //foreach (var device in devices)
                //    MessageBox.Show(device.friendlyName);
            }
            catch (Exception e)
            {

                Log.Error("", e);
            }
        
            return false;
        }


        #region IMMNotificationClient

        public void OnDeviceStateChanged(string deviceId, DeviceState newState) => OnAvailabilityChanged(null);

        public void OnDeviceAdded(string pwstrDeviceId) => OnAvailabilityChanged(null);

        public void OnDeviceRemoved(string deviceId) => OnAvailabilityChanged(null);

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) => OnAvailabilityChanged(null);

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) => OnAvailabilityChanged(null);

        #endregion
    }
}
