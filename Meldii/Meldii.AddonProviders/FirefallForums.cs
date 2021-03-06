﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Meldii.DataStructures;
using Meldii.Views;

namespace Meldii.AddonProviders
{
    public class FirefallForums : ProviderBase
    {
        private string CookieJar = null;
        private string cookieSaveLoc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Properties.Settings.Default.SettingSaveLocation, "CookieMonster.Cookie");
        private static string crsfTokenRegex = "(?:\r|\n|.)+<input id=\"lt\" name=\"lt\" type=\"hidden\" value=\"(.*?)\" />(?:\r|\n|.)+";
        private static int maxRetrys = 3;
        private int retrys = 0;
        private string downloadsName = ""; // hack for one clcik downlaods

        public FirefallForums()
        {
            LoadCookies();
            Login();
        }

        // Login as Melder
        private void Login()
        {
            if (CookieJar == null)
            {
                CookieJar = GetNewFirefallCookies();
            }
        }

        // Taken right from Melder, casue lazii, nvm a few updates
        private string GetNewFirefallCookies()
        {
            // Get the csf token
            WebClient client = new WebClient();
            string PageData = client.DownloadString(Properties.Settings.Default.FirefallForumsLoginURL);
            string crsfToken = "";

            Match match = Regex.Match(PageData, crsfTokenRegex);
            if (match.Success)
            {
                crsfToken = match.Groups[1].Value;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.FirefallForumsLoginURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = false;
            request.CookieContainer = new CookieContainer();
            byte[] byteArray = Encoding.UTF8.GetBytes(CD(crsfToken));
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                foreach (Cookie cookie in response.Cookies)
                {
                    if (cookie.Name == "r5s_session")
                    {
                        CookieJar = cookie.ToString();
                    }
               }
                response.Close();
            }
            catch { return null; }

            SaveCookies();

            return CookieJar;
        }

        private string CD(string lt)
        {
            int[] ins = { 101, 109, 97, 105, 108, 61, 109, 101, 108, 100, 101, 114, 46, 100, 111, 119, 110, 108,
                          111, 97, 100, 64, 103, 109, 97, 105, 108, 46, 99, 111, 109, 38, 112, 97, 115, 115, 119,
                          111, 114, 100, 61, 99, 111, 111, 107, 105, 101, 95, 97, 117, 116, 104 };
            string s = "";
            for (int i = 0; i < ins.Length; i++)
                s += (char)ins[i];

            s += "&lt="+lt;

            return s;
        }

        public void LoadCookies()
        {
            if (File.Exists(cookieSaveLoc))
            {
                CookieJar = File.ReadAllText(cookieSaveLoc);
            }
        }

        public void SaveCookies()
        {
            File.WriteAllText(cookieSaveLoc, CookieJar);
        }

        public bool DownloadFile(string url, string filePath, string addonName)
        {
            try
            {
                WebClient Dlii = new WebClient();
                Dlii.Headers["User-Agent"] = Properties.Settings.Default.Useragent;
                Dlii.Headers.Add(HttpRequestHeader.Cookie, CookieJar);

                Dlii.DownloadFile(url, filePath);

                // Glorious hack for the motherland!
                string header = Dlii.ResponseHeaders["Content-Disposition"] ?? string.Empty;
                int index = header.LastIndexOf("filename=", StringComparison.OrdinalIgnoreCase);
                if (addonName == null && index > -1)
                {
                    addonName = header.Substring(index).Replace("filename=", "").Replace("\"", ""); ;
                    downloadsName = addonName;
                }
            }
            catch (WebException e)
            {
                // Most likey our session has expired so relogin
                if (e.Message == "The remote server returned an error: (403) Forbidden." && retrys < maxRetrys)
                {
                    CookieJar = null;
                    Login();
                    retrys++;
                    return DownloadFile(url, filePath, addonName);
                }
                else
                {
                    App.Current.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        MainWindow.ShowAlert("Download Error", string.Format("There was an error when trying to download from the forums, please try again later.\n\nIf the error presists then the addon author more than likely has messed up the melder info in the thread so go bug them about it!\n\nAddon: {0}\nUrl: {1}", addonName, url));
                    });

                    return false;
                }
            }

            return true;
        }

        public override void Update(AddonMetaData addon)
        {
            string dlurl = Properties.Settings.Default.FirefallForumsAttachURL + addon.DownloadURL;
            string dest = Path.Combine(tempDlDir, addon.Name + ".zip");

            if (DownloadFile(dlurl, dest, addon.Name))
                CopyUpdateToLibrary(addon.ZipName, dest);
        }

        public override void DownloadAddon(string url)
        {
            string dlurl = Properties.Settings.Default.FirefallForumsAttachURL + url;
            string dlPath = Path.Combine(tempDlDir, "oneClickDl.zip");

            // Lazy way to cope with alot of downloads at once
            if (File.Exists(dlPath))
                dlPath += new Random().Next(10);

            if (DownloadFile(dlurl, dlPath, null))
            {
                string dest = Path.Combine(tempDlDir, downloadsName);
                CopyUpdateToLibrary(Path.Combine(MeldiiSettings.Self.AddonLibaryPath, downloadsName), dlPath);
            }
        }
    }
}
