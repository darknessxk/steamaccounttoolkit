using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAccountToolkit.ViewModels
{
    public class UsersListViewModel : BindableBase
    {
        public List<SteamUser> Users { get; set; } = new List<SteamUser>();

        public UsersListViewModel()
        {
            Users.Add(new SteamUser { User = "Teste" });
        }
    }
}
