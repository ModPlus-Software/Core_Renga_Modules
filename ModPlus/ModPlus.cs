namespace ModPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Helpers;
    using Microsoft.Win32;
    using ModPlusAPI;
    using ModPlusAPI.Enums;
    using ModPlusAPI.LicenseServer;
    using ModPlusAPI.UserInfo;
    using ModPlusAPI.Windows;

    /// <inheritdoc />
    public class ModPlus : Renga.IPlugin
    {
        public Renga.Application RengaApplication { get; private set; }

        public List<Renga.ActionEventSource> ActionEventSources { get; private set; }

        /// <inheritdoc/>
        public bool Initialize(string pluginFolder)
        {
            RengaApplication = new Renga.Application();
            ActionEventSources = new List<Renga.ActionEventSource>();
            try
            {
                // init lang
                if (!Language.Initialize())
                    return false;

                // statistic
                Statistic.SendModuleLoaded("Renga", "4.0");

                // Принудительная загрузка сборок
                LoadAssemblies();
                UserConfigFile.InitConfigFile();
                LoadFunctions();

                MenuBuilder.Build(RengaApplication, ActionEventSources);

                // проверка загруженности модуля автообновления
                CheckAutoUpdaterLoaded();

                var disableConnectionWithLicenseServer = Variables.DisableConnectionWithLicenseServerInRenga;

                // license server
                if (Variables.IsLocalLicenseServerEnable && !disableConnectionWithLicenseServer)
                    ClientStarter.StartConnection(SupportedProduct.Renga);
                
                if (Variables.IsWebLicenseServerEnable && !disableConnectionWithLicenseServer)
                    WebLicenseServerClient.Instance.Start(SupportedProduct.Renga);

                // user info
                AuthorizationOnStartup();

                return true;
            }
            catch (Exception exception)
            {
                RengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            foreach (var actionEventSource in ActionEventSources)
            {
                actionEventSource.Dispose();
            }

            ClientStarter.StopConnection();
        }

        /// <summary>
        /// Принудительная загрузка сборок необходимых для работы
        /// </summary>
        private void LoadAssemblies()
        {
            try
            {
                foreach (var fileName in Constants.ExtensionsLibraries)
                {
                    var extDll = Path.Combine(Constants.ExtensionsDirectory, fileName);
                    if (File.Exists(extDll))
                    {
                        Assembly.LoadFrom(extDll);
                    }
                }
            }
            catch (Exception exception)
            {
                RengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
            }
        }

        // Загрузка функций
        private void LoadFunctions()
        {
            try
            {
                var functionsKey = Registry.CurrentUser.OpenSubKey("ModPlus\\Functions");
                if (functionsKey == null)
                    return;
                using (functionsKey)
                {
                    foreach (var functionKeyName in functionsKey.GetSubKeyNames())
                    {
                        var functionKey = functionsKey.OpenSubKey(functionKeyName);
                        var prForValue = functionKey?.GetValue("ProductFor");
                        if (prForValue != null && prForValue.Equals("Renga"))
                        {
                            // беру свойства функции из реестра
                            var file = functionKey.GetValue("File") as string;
                            var onOff = functionKey.GetValue("OnOff") as string;
                            if (string.IsNullOrEmpty(onOff))
                                continue;
                            var isOn = !bool.TryParse(onOff, out var b) || b; // default - true
                            
                            // Если "Продукт для" подходит, файл существует и функция включена - гружу
                            if (isOn)
                            {
                                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                                {
                                    // load
                                    var localFuncAssembly = Assembly.LoadFrom(file);
                                    LoadPluginsHelper.GetDataFromFunctionInterface(localFuncAssembly, file);
                                }
                                else
                                {
                                    var foundedFile = LoadPluginsHelper.FindFile(functionKeyName);
                                    if (!string.IsNullOrEmpty(foundedFile) && File.Exists(foundedFile))
                                    {
                                        var localFuncAssembly = Assembly.LoadFrom(foundedFile);
                                        LoadPluginsHelper.GetDataFromFunctionInterface(localFuncAssembly, foundedFile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
            }
        }

        /// <summary>
        /// Проверка загруженности модуля автообновления
        /// </summary>
        private void CheckAutoUpdaterLoaded()
        {
            try
            {
                var loadWithWindows = !bool.TryParse(RegistryUtils.GetValue("AutoUpdater", "LoadWithWindows"), out var b) || b;
                if (loadWithWindows)
                {
                    // Если "грузить с виндой", то проверяем, что модуль запущен
                    // если не запущен - запускаем
                    var isOpen = Process.GetProcesses().Any(t => t.ProcessName == "mpAutoUpdater");
                    if (!isOpen)
                    {
                        var fileToStart = Path.Combine(Constants.CurrentDirectory, "mpAutoUpdater.exe");
                        if (File.Exists(fileToStart))
                        {
                            Process.Start(fileToStart);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Statistic.SendException(exception);
            }
        }

        private async void AuthorizationOnStartup()
        {
            try
            {
                await UserInfoService.GetUserInfoAsync();
                var userInfo = UserInfoService.GetUserInfoResponseFromHash();
                if (userInfo != null)
                {
                    if (!userInfo.IsLocalData && !await ModPlusAPI.Web.Connection.HasAllConnectionAsync(3))
                    {
                        Variables.UserInfoHash = string.Empty;
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }
    }
}
