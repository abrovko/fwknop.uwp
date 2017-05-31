using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace fwknop.uwp.Ui
{
    class UiSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName + "Valid"));
        }
        public string SpaServer
        {
            get { return ApplicationData.Current.LocalSettings.Values[nameof(SpaServer)] as string; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(SpaServer)] = value; OnPropertyChanged(); }
        }
        public bool SpaServerValid { get { return !string.IsNullOrWhiteSpace(SpaServer); } }

        public int SpaServerPort
        {
            get { return (ApplicationData.Current.LocalSettings.Values[nameof(SpaServerPort)] as int?) ?? 62201; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(SpaServerPort)] = value; OnPropertyChanged(); }
        }
        public bool SpaServerPortValid { get { return SpaServerPort > 0 && SpaServerPort <= 65535; } }

        public string Base64EncryptionKey
        {
            get { return ApplicationData.Current.LocalSettings.Values[nameof(Base64EncryptionKey)] as string; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(Base64EncryptionKey)] = value; OnPropertyChanged(); }
        }
        public bool Base64EncryptionKeyValid { get { return IsBase64Valid(Base64EncryptionKey); } }


        public string Base64HmacKey
        {
            get { return ApplicationData.Current.LocalSettings.Values[nameof(Base64HmacKey)] as string; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(Base64HmacKey)] = value; OnPropertyChanged(); }
        }
        public bool Base64HmacKeyValid { get { return IsBase64Valid(Base64HmacKey); } }

        public string ProtocolPort
        {
            get { return ApplicationData.Current.LocalSettings.Values[nameof(ProtocolPort)] as string ?? "tcp/22"; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(ProtocolPort)] = value; OnPropertyChanged(); }
        }
        public bool ProtocolPortValid { get { return !string.IsNullOrWhiteSpace(ProtocolPort); } }


        public string AllowIp
        {
            get { return ApplicationData.Current.LocalSettings.Values[nameof(AllowIp)] as string; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(AllowIp)] = value; OnPropertyChanged(); }
        }
        public bool AllowIpValid { get { return !string.IsNullOrWhiteSpace(AllowIp); } }


        public string NatAccess
        {
            get { return ApplicationData.Current.LocalSettings.Values[nameof(NatAccess)] as string; }
            set { ApplicationData.Current.LocalSettings.Values[nameof(NatAccess)] = value; OnPropertyChanged(); }
        }
        public bool NatAccessValid { get { return true; } }


        public bool IsSettingsValid()
        {
            //some basic validation
            return Base64HmacKeyValid
                && Base64EncryptionKeyValid
                && SpaServerValid
                && SpaServerPortValid;
        }
        public bool IsCommandValid()
        {
            return AllowIpValid
                && ProtocolPortValid
                && NatAccessValid;
        }

        bool IsBase64Valid(string b64)
        {
            if (string.IsNullOrWhiteSpace(b64))
                return false;
            try
            {
                Convert.FromBase64String(b64);
                return true;
            }
            catch { return false; }
        }
    }
}
