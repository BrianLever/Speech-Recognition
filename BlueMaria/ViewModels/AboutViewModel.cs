using BlueMaria.Services.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlueMaria.ViewModels
{
    class AboutViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Properties

        //private string _productVersion;
        public string ProductVersion
        {
            get
            {
                return RegistryService.ReadRegistryEntry("ProductVersion");
            }
            //set
            //{
            //    if (this._logText == value) return;
            //    this._logText = value;
            //    this.OnPropertyChanged(nameof(LogText));
            //    //this.OnLogText();
            //}
        }
        
        #endregion

    }
}
