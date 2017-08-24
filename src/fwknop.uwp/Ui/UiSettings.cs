using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace fwknop.uwp.Ui
{


    class UiSettings : INotifyPropertyChanged
    {
        static readonly char[] Separator = new char[] { '|' };
        public UiSettings()
        {
            var p = (StoredValues[nameof(Profiles)] as string ?? "").Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            if (!p.Any())
            {
                this.CurrentProfile = "Default";
                this.Profiles = new ObservableCollection<string>(new string[] { this.CurrentProfile });
            }
            else
            {
                this.Profiles = new ObservableCollection<string>(p.OrderBy(i => i));
                this.CurrentProfile = StoredValues[nameof(CurrentProfile)] as string ?? "Default";
            }
            AllowIpHistory = new ObservableCollection<string>((StoredValues["AllowIpHistory"] as string ?? "").Split(Separator, StringSplitOptions.RemoveEmptyEntries));
            LoadProfile();
        }
        public string SpaServer { get { return StoredValues[nameof(SpaServer)] as string; } set { SetValue(value); } }
        public bool SpaServerValid { get { return !string.IsNullOrWhiteSpace(SpaServer); } }

        public int SpaServerPort { get { return (StoredValues[nameof(SpaServerPort)] as int?) ?? 62201; } set { SetValue(value); } }
        public bool SpaServerPortValid { get { return SpaServerPort > 0 && SpaServerPort <= 65535; } }

        public string Base64EncryptionKey { get { return StoredValues[nameof(Base64EncryptionKey)] as string; } set { SetValue(value); } }
        public bool Base64EncryptionKeyValid { get { return IsBase64Valid(Base64EncryptionKey); } }

        public string Base64HmacKey { get { return StoredValues[nameof(Base64HmacKey)] as string; } set { SetValue(value); } }
        public bool Base64HmacKeyValid { get { return IsBase64Valid(Base64HmacKey); } }

        public ObservableCollection<string> Profiles { get; private set; }
        public string CurrentProfile { get; set; }

        public string AllowIp { get { return StoredValues[nameof(AllowIp)] as string; } set { SetValue(value); } }
        public bool AllowIpValid { get { return !string.IsNullOrWhiteSpace(AllowIp); } }
        public ObservableCollection<string> AllowIpHistory { get; private set; }

        //{ return ; } set { SetValue(value, $"ProtocolPort.{CurrentProfile}"); 
        public string ProtocolPort { get; set; }
        public bool ProtocolPortValid { get { return !string.IsNullOrWhiteSpace(ProtocolPort); } }

        public string NatAccess { get; set; }



        public bool NatAccessValid { get { return true; } }

        public void SaveCurrentProfile()
        {
            if (!AllowIpHistory.Contains(AllowIp))
            {
                AllowIpHistory.Add(AllowIp);
                SetValue(string.Join("|", AllowIpHistory), $"AllowIpHistory");
            }

            StoredValues[nameof(CurrentProfile)] = CurrentProfile;
            if (!Profiles.Contains(CurrentProfile))
            {
                Profiles.Add(CurrentProfile);
                StoredValues[nameof(Profiles)] = string.Join("|", Profiles);
            }
            SetValue(ProtocolPort, $"ProtocolPort.{CurrentProfile}");
            SetValue(NatAccess, $"NatAccess.{CurrentProfile}");
        }
        public void LoadProfile()
        {
            ProtocolPort = StoredValues[$"ProtocolPort.{CurrentProfile}"] as string ?? "tcp/22";
            NatAccess = StoredValues[$"NatAccess.{CurrentProfile}"] as string;
            this.OnPropertyChanged(nameof(ProtocolPort));
            this.OnPropertyChanged(nameof(NatAccess));
        }
        public void DeleteCurrentProfile()
        {
            if (Profiles.Count <= 1)
                return;
            StoredValues.Remove($"ProtocolPort.{CurrentProfile}");
            StoredValues.Remove($"NatAccess.{CurrentProfile}");
            Profiles.Remove(CurrentProfile);
            CurrentProfile = Profiles[0];
            this.OnPropertyChanged(nameof(CurrentProfile));
            LoadProfile();
            SaveCurrentProfile();
        }
        public void DeleteCurrentIp()
        {
            if (AllowIpHistory.Remove(AllowIp))
            {
                AllowIp = AllowIpHistory.FirstOrDefault();
                SaveCurrentProfile();
            }
        }

        #region Validation
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
        #endregion

        #region Utils
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName + "Valid"));
        }
        private IPropertySet StoredValues => ApplicationData.Current.LocalSettings.Values;
        private void SetValue(object value, [CallerMemberName] string propertyName = null)
        {
            StoredValues[propertyName] = value;
            OnPropertyChanged(propertyName);
        }

        #endregion
    }
}
