using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using System.Threading;
using System.Diagnostics;
using MahApps.Metro.Controls.Dialogs;
using System.Security;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SteamAutoLogin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        Thread Thread_LoginWatcher;

        bool IsAppRunning;

        AddNew addNewWindow;

        private ObservableCollection<SteamUser> Users;

        public MainWindow()
        {
            App.AppMain = this;
            InitializeComponent();

            Users = new ObservableCollection<SteamUser>();

            UsersList.ItemsSource = Users;

            Thread_LoginWatcher = new Thread(LoginWatchDog);

            Thread_LoginWatcher.Start();
            IsAppRunning = true;

            Closing += (se, es) => IsAppRunning = false;

            addNewWindow = new AddNew();
            bool isClosing = false;

            addNewWindow.Closing += (s, e) =>
            {
                if(!isClosing)
                {
                    e.Cancel = true;
                    addNewWindow.Hide();
                }
            };

            Closing += (s, e) =>
            {
                isClosing = true;

                addNewWindow.Close();
            };
        }

        public void AddNewUser(LoginData user)
        {
            Users.Add(new SteamUser
            {
                UserLogin = user
            });
        }

        public void RemoveUser(LoginData user)
        {
            Users.RemoveAt(Users.IndexOf(Users.First(x => x.UserLogin == user)));
        }

        private void LoginWatchDog()
        {
            while (IsAppRunning)
            {
                if (Process.GetProcessesByName("steam").Count() > 0)
                {
                    Dispatcher.Invoke(() => { SteamIsOpen.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); });

                    if (SteamDesktopApi.GetSteamMainWindow() != IntPtr.Zero)
                    {
                        if(IsAppRunning)
                            Dispatcher.Invoke(() => { SteamInMain.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); });
                    }
                    else
                    {
                        if (IsAppRunning)
                            Dispatcher.Invoke(() => { SteamInMain.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); });
                    }

                    if (SteamDesktopApi.GetSteamLoginWindow() != IntPtr.Zero)
                    {
                        if (IsAppRunning)
                            Dispatcher.Invoke(() => { SteamCanLogin.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); });
                    }
                    else
                    {
                        if (IsAppRunning)
                            Dispatcher.Invoke(() => { SteamCanLogin.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); });
                    }

                    if (SteamDesktopApi.GetSteamGuardWindow() != IntPtr.Zero)
                    {
                        if (IsAppRunning)
                            Dispatcher.Invoke(() => { SteamGuardReq.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); });
                    }
                    else
                    {
                        if (IsAppRunning)
                            Dispatcher.Invoke(() => { SteamGuardReq.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); });
                    }
                }
                else
                if (IsAppRunning)
                    Dispatcher.Invoke(() => { SteamIsOpen.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); });
            }
        }

        private void BtnAddNew(object sender, RoutedEventArgs e)
        {
            addNewWindow.Show();
        }
    }
}
