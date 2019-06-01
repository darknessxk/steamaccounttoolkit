using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SteamAutoLogin
{
    /// <summary>
    /// Interaction logic for SteamUser.xaml
    /// </summary>
    public partial class SteamUser : UserControl
    {
        private LoginData _login;
        public LoginData UserLogin {
            get
            {
                return _login;
            }
            set
            {
                _login = value;

                if (_login.SteamId64 != string.Empty)
                {
                    Task.Run(() =>
                    {
                        var url = SteamDesktopApi.GetProfileIconUrl(_login);
                        var bData = new WebClient().DownloadData(url);

                        HtmlDocument doc = new HtmlWeb().Load(new Uri($"https://steamcommunity.com/profiles/{_login.SteamId64}"));
                        var personaName = doc.DocumentNode.Descendants().Where(n => n.HasClass("actual_persona_name")).First().GetDirectInnerText();

                        Dispatcher.Invoke(() =>
                        {
                            MemoryStream ms = new MemoryStream(bData);
                            BitmapImage img = new BitmapImage();
                            img.BeginInit();
                            img.StreamSource = ms;
                            img.EndInit();

                            AccountAlias.Content = personaName;
                            BackgroundImage.ImageSource = img;
                        });
                    });
                }
            }
        }

        public SteamUser()
        {
            InitializeComponent();
        }

        private void BtnClickRemove(object sender, RoutedEventArgs e)
        {
            App.AppMain.RemoveUser(_login);
        }

        private void BtnClickLogin(object sender, RoutedEventArgs e)
        {
            if(UserLogin.User != string.Empty)
            {
                if(SteamDesktopApi.IsOnMainWindow())
                    SteamDesktopApi.QuitSteam();

                Task.Run(() =>
                {
                    while(System.Diagnostics.Process.GetProcessesByName("Steam").Length > 0)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    
                    SteamDesktopApi.SteamLogin(UserLogin);
                });
            }
        }
    }
}
