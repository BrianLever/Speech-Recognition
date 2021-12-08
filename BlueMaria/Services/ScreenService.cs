using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMaria.Services
{
    public class ScreenService : IScreenService
    {
        private readonly Action<bool> _onNetworkAvailableChanged;
        private readonly Action<bool> _onCanRecordChanged;

        public ScreenService(Action<bool> onNetworkAvailableChanged, Action<bool> onCanRecordChanged)
        {
            _onNetworkAvailableChanged = onNetworkAvailableChanged;
            _onCanRecordChanged = onCanRecordChanged;
        }

        public void ShowCanRecord(bool canRecord)
        {
            _onCanRecordChanged(canRecord);
        }

        public void ShowNetworkAvailable(bool isAvailable)
        {
            _onNetworkAvailableChanged(isAvailable);
        }
    }
}
