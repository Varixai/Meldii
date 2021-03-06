﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Meldii
{
    public class Program
    {
        [STAThreadAttribute]
        public static void Main(string[] arguments)
        {
            string args = "";
            for (int i = 0; i < arguments.Length; i++)
                args += arguments[i] + " ";
            args = args.Trim();

            Statics.LaunchArgs = args;

            ParseProtcol(args);

            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ErrorHandler);
            App.Main();
        }

        static void ParseProtcol(string args)
        {
            try
            {
                Match match = Regex.Match(args, Statics.MelderProtcolRegex);
                if (match.Success)
                {
                    string action = match.Groups[1].Value;
                    string provider = match.Groups[2].Value;
                    string url = match.Groups[3].Value;

                    if (action == "download")
                    {
                        if (provider == "forum") // Backwards Melder compat
                        {
                            Statics.OneClickInstallProvider = AddonProviders.AddonProviderType.FirefallForums;
                            Statics.OneClickAddonToInstall = url;
                        }
                        else // New stuff
                        {
                            try
                            {
                                Statics.OneClickInstallProvider = (AddonProviders.AddonProviderType)Enum.Parse(typeof(AddonProviders.AddonProviderType), provider, true);
                                Statics.OneClickAddonToInstall = url;
                            }
                            catch (Exception e)
                            {
                                
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        static void ErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageBox.Show(e.StackTrace, e.Message);
            string[] lines = 
            {
                ".Net Runtime Version: " + Environment.Version.ToString(),
                "Source: " + e.Source,
                "Target: " + e.TargetSite,
                "Message: " + e.Message,
                "\n",
                e.StackTrace,
                "\n"
            };

            System.IO.File.WriteAllLines(@"Meldii Errors make Arkii sad.txt", lines);
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
