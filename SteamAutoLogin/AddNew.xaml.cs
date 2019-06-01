using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace SteamAutoLogin
{
    /// <summary>
    /// Interaction logic for AddNew.xaml
    /// </summary>
    public partial class AddNew : MetroWindow
    {
        public AddNew()
        {
            InitializeComponent();
        }

        //CANCEL
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        //ADD
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            App.AppMain.AddNewUser(new LoginData
            {
                User = username.Text,
                Pass = password.Password,
                SteamId64 = steamid64.Text,
                SteamGuardPrivateKey = steamguard.Password,
            });

            username.Text = string.Empty;
            password.Password = string.Empty;
            steamid64.Text = string.Empty;
            steamguard.Password = string.Empty;

            Hide();

        }
    }
}
