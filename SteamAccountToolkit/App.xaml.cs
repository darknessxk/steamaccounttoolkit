using Unity;
using Prism.Unity;
using System.Windows;
using Prism.Ioc;

namespace SteamAccountToolkit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.AppMain>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Views.UsersList>("UsersList");
        }
    }
}