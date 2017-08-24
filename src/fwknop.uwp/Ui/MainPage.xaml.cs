using fwknop.uwp.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace fwknop.uwp
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HttpClient GetMyIpHttpClient = new HttpClient();
        UiSettings Settings = new UiSettings();
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Settings.IsSettingsValid())
            {
                this.Frame.Navigate(typeof(SettingsPage));
                return;
            }
            if (!Settings.IsCommandValid())
            {
                return; //TODO: highlight?
            }
            try
            {
                Settings.SaveCurrentProfile();

                var spaGen = new SpaGenerator(Settings.Base64EncryptionKey, Settings.Base64HmacKey);
                var msg = spaGen.CreateSpaPacket(Settings.ProtocolPort, Settings.AllowIp, Settings.NatAccess);
                var sentBytes = await SocketUtil.SendAsync(Settings.SpaServer, Settings.SpaServerPort, msg);
                Log.Text = $"{sentBytes} bytes sent.\n" + Log.Text;
            }
            catch (Exception ex)
            {
                await LogError(ex);
            }
        }

        private async void GetMyIpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var res = await GetMyIpHttpClient.GetAsync("https://www.cipherdyne.org/cgi-bin/myip");
                res.EnsureSuccessStatusCode();
                var ip = await res.Content.ReadAsStringAsync();
                Settings.AllowIp = ip.Trim(' ', '\r', '\n');
            }
            catch (Exception ex)
            {
                await LogError(ex);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        async Task LogError(Exception ex)
        {
            Log.Text += ex.ToString() + "\n";
            var dialog = new MessageDialog(ex.Message);
            await dialog.ShowAsync();
        }
            
        private void DeleteCurrentProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.DeleteCurrentProfile();
        }
        private void DeleteCurrentIpButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.DeleteCurrentIp();
        }

        private void CurrentProfile_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Settings.CurrentProfile = args.SelectedItem as string;
            Settings.LoadProfile();
        }
    }
}
