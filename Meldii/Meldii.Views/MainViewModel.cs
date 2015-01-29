﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Meldii.AddonProviders;

namespace Meldii.Views
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AddonMetaData> LocalAddons { get; set; }

        private string _StatusMessage;
        public string StatusMessage { get { return _StatusMessage; } set { _StatusMessage = value; NotifyPropertyChanged("StatusMessage"); } }

        public int SelectedAddonIndex = -1;
        public AddonMetaData _SelectedAddon;
        public AddonMetaData SelectedAddon { get { return _SelectedAddon; } set { _SelectedAddon = value; NotifyPropertyChanged("SelectedAddon"); } }
        public bool _IsPendingVersionCheck = true;

        public SettingsView _SettingsView;
        public SettingsView SettingsView { get { return _SettingsView; } set { _SettingsView = value; NotifyPropertyChanged("SettingsView"); } }
        public MainViewModel()
        {
            SettingsView = new SettingsView();

            StatusMessage = "Checking for new addon updates.....";
            LocalAddons = new ObservableCollection<AddonMetaData>();
        }

        public bool IsPendingVersionCheck
        {
            get
            {
                return _IsPendingVersionCheck;
            }

            set
            {
                _IsPendingVersionCheck = value;
                NotifyPropertyChanged("IsPendingVersionCheck");
            }
        }

        public void OnOpenAddonPage()
        {
            if (SelectedAddon != null && SelectedAddon.AddonPage != null && SelectedAddon.AddonPage.Length > 0)
                Process.Start(SelectedAddon.AddonPage);
        }

        public void OnSelectedAddon(int SelectedIdx)
        {
            SelectedAddonIndex = SelectedIdx;
            if (SelectedIdx >= 0 && SelectedIdx < LocalAddons.Count)
            {
                SelectedAddon = LocalAddons[SelectedIdx];
            }
        }

        public void UpdateAddon()
        {
            if (SelectedAddonIndex >= 0 && SelectedAddonIndex < LocalAddons.Count)
            {
                AddonManager.Self.UpdateAddon(SelectedAddon);
            }
        }

        public async void AddonDeleteFromLibrary()
        {
            if (SelectedAddonIndex >= 0 && SelectedAddonIndex < LocalAddons.Count)
            {
                if (await MainWindow.ShowMessageDialogYesNo("Are you sure?", string.Format("This will delete {0} Version: {1} from your addon Library.", SelectedAddon.Name, SelectedAddon.Version)))
                {
                    AddonManager.Self.DeleteAddonFromLibrary(SelectedAddonIndex);
                }
            }
        }

        public static string AssemblyVersion
        {
            get
            {
                return "Version: " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            }
        }

        // Gah
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
