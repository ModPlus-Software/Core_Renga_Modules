namespace ModPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Win32;
    using ModPlusAPI;
    using Renga;

    public class ModPlus : IPlugin
    {
        public Application RengApplication { get; private set; }

        public List<ActionEventSource> ActionEventSources { get; private set; }

        public bool Initialize(string pluginFolder)
        {
            RengApplication = new Renga.Application();
            ActionEventSources = new List<ActionEventSource>();
            try
            {
                // inint lang
                if (!Language.Initialize())
                    return false;

                // statistic
                ////Statistic.SendPluginStarting("Renga", MpVersionData.CurRevitVers);

                // Принудительная загрузка сборок
                LoadAssms();
                UserConfigFile.InitConfigFile();
                LoadFunctions();

                TestLoadCommand();

                // проверка загруженности модуля автообновления
                CheckAutoUpdaterLoaded();

                return true;
            }
            catch (Exception exception)
            {
                RengApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
                return false;
            }
        }

        public void Stop()
        {
            foreach (var actionEventSource in ActionEventSources)
            {
                actionEventSource.Dispose();
            }
        }

        private void TestLoadCommand()
        {
            var uiPanelExtension = RengApplication.UI.CreateUIPanelExtension();
            string file = @"D:\ModPlus\Functions\Renga\RengaTestFunction\RengaTestFunction.dll";
            var assembly = Assembly.LoadFrom(file);

            foreach (var t in assembly.GetTypes())
            {
                var c = t.GetInterface(typeof(IRengaFunction).Name);
                if (c != null && Activator.CreateInstance(t) is IRengaFunction function)
                {
                    var action = RengApplication.UI.CreateAction();
                    action.ToolTip = "Test";
                    ActionEventSource actionEventSource = new ActionEventSource(action);
                    ActionEventSources.Add(actionEventSource);
                    actionEventSource.Triggered += (sender, args) => function.Start();
                    uiPanelExtension.AddToolButton(action);
                }
            }

            RengApplication.UI.AddExtensionToPrimaryPanel(uiPanelExtension);
        }

        /// <summary>
        /// Принудительная загрузка сборок необходимых для работы
        /// </summary>
        private void LoadAssms()
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
                RengApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
            }
        }

        // Загрузка функций
        private void LoadFunctions()
        {
            try
            {
                var funtionsKey = Registry.CurrentUser.OpenSubKey("ModPlus\\Functions");
                if (funtionsKey == null)
                    return;
                using (funtionsKey)
                {
                    foreach (var functionKeyName in funtionsKey.GetSubKeyNames())
                    {
                        var functionKey = funtionsKey.OpenSubKey(functionKeyName);
                        if (functionKey == null)
                            continue;
                        foreach (var availPrVersKeyName in functionKey.GetSubKeyNames())
                        {
                            // Если версия продукта не совпадает, то пропускаю
                            ////if (!availPrVersKeyName.Equals(MpVersionData.CurRevitVers))
                            ////    continue;
                            var availPrVersKey = functionKey.OpenSubKey(availPrVersKeyName);
                            if (availPrVersKey == null)
                                continue;

                            // беру свойства функции из реестра
                            var file = availPrVersKey.GetValue("File") as string;
                            var onOff = availPrVersKey.GetValue("OnOff") as string;
                            var productFor = availPrVersKey.GetValue("ProductFor") as string;
                            if (string.IsNullOrEmpty(onOff) || string.IsNullOrEmpty(productFor))
                                continue;
                            if (!productFor.Equals("Renga"))
                                continue;
                            var isOn = !bool.TryParse(onOff, out var b) || b; // default - true

                            // Если "Продукт для" подходит, файл существует и функция включена - гружу
                            if (isOn)
                            {
                                ////if (!string.IsNullOrEmpty(file) && File.Exists(file))
                                ////{
                                ////    // load
                                ////    var localFuncAssembly = Assembly.LoadFrom(file);
                                ////    LoadFunctionsHelper.GetDataFromFunctionIntrface(localFuncAssembly, file);
                                ////}
                                ////else
                                ////{
                                ////    var findedFile = LoadFunctionsHelper.FindFile(functionKeyName);
                                ////    if (!string.IsNullOrEmpty(findedFile) && File.Exists(findedFile))
                                ////    {
                                ////        var localFuncAssembly = Assembly.LoadFrom(findedFile);
                                ////        LoadFunctionsHelper.GetDataFromFunctionIntrface(localFuncAssembly, findedFile);
                                ////    }
                                ////}
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RengApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
            }
        }

        /// <summary>
        /// Проверка загруженности модуля автообновления
        /// </summary>
        private void CheckAutoUpdaterLoaded()
        {
            try
            {
                var loadWithWindows = !bool.TryParse(Regestry.GetValue("AutoUpdater", "LoadWithWindows"), out var b) || b;
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
    }
}
