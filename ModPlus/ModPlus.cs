﻿namespace ModPlus
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
    using Renga;

    public class ModPlus : IPlugin
    {
        public Application RengaApplication { get; private set; }

        public List<ActionEventSource> ActionEventSources { get; private set; }

        public bool Initialize(string pluginFolder)
        {
            RengaApplication = new Renga.Application();
            ActionEventSources = new List<ActionEventSource>();
            try
            {
                // init lang
                if (!Language.Initialize())
                    return false;

                // statistic
                ////Statistic.SendPluginStarting("Renga", MpVersionData.CurRevitVers);

                // Принудительная загрузка сборок
                LoadAssemblies();
                UserConfigFile.InitConfigFile();
                LoadFunctions();

                MenuBuilder.Build(RengaApplication, ActionEventSources);

                // проверка загруженности модуля автообновления
                CheckAutoUpdaterLoaded();

                return true;
            }
            catch (Exception exception)
            {
                RengaApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
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
            var uiPanelExtension = RengaApplication.UI.CreateUIPanelExtension();
            string file = @"D:\ModPlus\Functions\Renga\RengaTestFunction\RengaTestFunction.dll";
            var assembly = Assembly.LoadFrom(file);

            foreach (var t in assembly.GetTypes())
            {
                var c = t.GetInterface(typeof(IRengaFunction).Name);
                if (c != null && Activator.CreateInstance(t) is IRengaFunction function)
                {
                    var action = RengaApplication.UI.CreateAction();
                    action.ToolTip = "Test";
                    ActionEventSource actionEventSource = new ActionEventSource(action);
                    ActionEventSources.Add(actionEventSource);
                    actionEventSource.Triggered += (sender, args) => function.Start();
                    uiPanelExtension.AddToolButton(action);
                }
            }

            RengaApplication.UI.AddExtensionToPrimaryPanel(uiPanelExtension);
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
                RengaApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
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
                        if (functionKey == null)
                            continue;
                        var prForValue = functionKey.GetValue("ProductFor");
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
                                    LoadFunctionsHelper.GetDataFromFunctionInterface(localFuncAssembly, file);
                                }
                                else
                                {
                                    var foundedFile = LoadFunctionsHelper.FindFile(functionKeyName);
                                    if (!string.IsNullOrEmpty(foundedFile) && File.Exists(foundedFile))
                                    {
                                        var localFuncAssembly = Assembly.LoadFrom(foundedFile);
                                        LoadFunctionsHelper.GetDataFromFunctionInterface(localFuncAssembly, foundedFile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RengaApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message);
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
