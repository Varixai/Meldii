﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Meldii.AddonProviders;
using Meldii.DataStructures;

namespace Meldii.Views
{
    public class SettingsView : INotifyPropertyChanged
    {
        private string _FirefallInstall;
        private string _AddonLibaryPath;

        public SettingsView()
        {
            FirefallInstall = MeldiiSettings.Self.FirefallInstallPath;
            AddonLibaryPath = MeldiiSettings.Self.AddonLibaryPath;
        }

        public void UpdateSettings()
        {
            FirefallInstall = MeldiiSettings.Self.FirefallInstallPath;
            AddonLibaryPath = MeldiiSettings.Self.AddonLibaryPath;
        }

        // Ui binding hooks
        public string FirefallInstall
        {
            get { return _FirefallInstall; }

            set
            {
                _FirefallInstall = value;
                NotifyPropertyChanged("FirefallInstall");
            }
        }

        public string AddonLibaryPath
        {
            get { return MeldiiSettings.Self.AddonLibaryPath; }

            set
            {
                _AddonLibaryPath = value;
                NotifyPropertyChanged("AddonLibaryPath");
            }
        }

        public void SaveSettings()
        {
            bool hasAddonLibFolderChanged = (MeldiiSettings.Self.AddonLibaryPath != _AddonLibaryPath);

            MeldiiSettings.Self.FirefallInstallPath = _FirefallInstall;
            MeldiiSettings.Self.AddonLibaryPath = _AddonLibaryPath;
            MeldiiSettings.Self.Save();

            if (hasAddonLibFolderChanged)
            {
                AddonManager.Self.GetLocalAddons();
                AddonManager.Self.SetupFolderWatchers();
            }
        }

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
