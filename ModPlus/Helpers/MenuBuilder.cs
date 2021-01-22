namespace ModPlus.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ModPlusAPI;
    using ModPlusAPI.Enums;
    using Renga;

    internal static class MenuBuilder
    {
        private const string LangItem = "RengaDlls";

        internal static void Build(Application rengaApplication, List<ActionEventSource> actionEventSources)
        {
            // sort functions by LName
            LoadPluginsHelper.LoadedFunctions.Sort((f1, f2) => string.Compare(f1.LName, f2.LName, StringComparison.Ordinal));

            var ui = rengaApplication.UI;

            // Create primary drop down menu
            var uiIPanelExtension = ui.CreateUIPanelExtension();
            var dropDownButton = ui.CreateDropDownButton();
            var extractedImageResource = ExtractResource(Assembly.GetExecutingAssembly(), "ModPlus_24x24.png");
            if (extractedImageResource != null)
            {
                var icon = ui.CreateImage();
                icon.LoadFromData(extractedImageResource, ImageFormat.ImageFormat_PNG);
                dropDownButton.Icon = icon;
            }

            // DropDownButton has it's own name and icon
            dropDownButton.ToolTip = "ModPlus";

            foreach (var loadedFunction in LoadPluginsHelper.LoadedFunctions)
            {
                if (loadedFunction.IsAddingToMenuBySelf)
                {
                    StartFunction(rengaApplication, loadedFunction);
                }
                else
                {
                    if (loadedFunction.UiLocation == RengaFunctionUILocation.PrimaryPanel)
                    {
                        dropDownButton.AddAction(GetActionForFunction(rengaApplication, actionEventSources, loadedFunction, false, true));
                    }
                    else if (loadedFunction.UiLocation == RengaFunctionUILocation.ActionPanel)
                    {
                        var action = GetActionForButtonInActionPanel(rengaApplication, actionEventSources, loadedFunction);
                        if (action != null)
                        {
                            var uiIPanelExtensionForActionPanel = ui.CreateUIPanelExtension();
                            uiIPanelExtensionForActionPanel.AddToolButton(action);
                        }
                    }
                    else
                    {
                        if (loadedFunction.ContextMenuShowCase == RengaContextMenuShowCase.Selection)
                        {
                            foreach (var viewType in loadedFunction.ViewTypes)
                            {
                                // context menu for selection
                                var contextMenuForSelection = ui.CreateContextMenu();
                                contextMenuForSelection.AddActionItem(GetActionForFunction(rengaApplication, actionEventSources, loadedFunction, true, false));
                                ui.AddContextMenu(
                                    Guid.NewGuid(), contextMenuForSelection, GetRengaViewType(viewType), ContextMenuShowCase.ContextMenuShowCase_Selection);
                            }
                        }
                        else
                        {
                            foreach (var viewType in loadedFunction.ViewTypes)
                            {
                                var contextMenuForScene = ui.CreateContextMenu();
                                contextMenuForScene.AddActionItem(GetActionForFunction(rengaApplication, actionEventSources, loadedFunction, true, false));
                                ui.AddContextMenu(
                                    Guid.NewGuid(), contextMenuForScene, GetRengaViewType(viewType), ContextMenuShowCase.ContextMenuShowCase_Selection);
                            }
                        }
                    }
                }
            }

            // add settings button to primary menu
            dropDownButton.AddSeparator();
            dropDownButton.AddAction(GetActionForPersonalAccountCommand(ui, actionEventSources));
            dropDownButton.AddAction(GetActionForFeedbackCommand(ui, actionEventSources));
            dropDownButton.AddAction(GetActionForSettingsCommand(ui, actionEventSources));

            uiIPanelExtension.AddDropDownButton(dropDownButton);

            // Add controls to the primary panel:
            ui.AddExtensionToPrimaryPanel(uiIPanelExtension);
        }

        /// <summary>
        /// Получить IAction для кнопки запуска Личного кабинета
        /// </summary>
        /// <param name="ui">The UI.</param>
        /// <param name="actionEventSources">The action event sources.</param>
        private static IAction GetActionForPersonalAccountCommand(IUI ui, ICollection<ActionEventSource> actionEventSources)
        {
            var action = ui.CreateAction();

            var extractedImageResource = ExtractResource(Assembly.GetExecutingAssembly(), "PersonalAccount_16x16.png");
            if (extractedImageResource != null)
            {
                var icon = ui.CreateImage();
                icon.LoadFromData(extractedImageResource, ImageFormat.ImageFormat_PNG);
                action.Icon = icon;
            }

            action.DisplayName = Language.GetItem(LangItem, "h7");

            var actionEventSource = new ActionEventSource(action);
            actionEventSource.Triggered += (s, e) =>
            {
                ModPlusAPI.UserInfo.UserInfoService.ShowUserInfo();
            };

            actionEventSources.Add(actionEventSource);

            return action;
        }

        /// <summary>
        /// Получить IAction для кнопки запуска настроек
        /// </summary>
        /// <param name="ui">The UI.</param>
        /// <param name="actionEventSources">The action event sources.</param>
        private static IAction GetActionForSettingsCommand(IUI ui, ICollection<ActionEventSource> actionEventSources)
        {
            var action = ui.CreateAction();

            var extractedImageResource = ExtractResource(Assembly.GetExecutingAssembly(), "Settings_16x16.png");
            if (extractedImageResource != null)
            {
                var icon = ui.CreateImage();
                icon.LoadFromData(extractedImageResource, ImageFormat.ImageFormat_PNG);
                action.Icon = icon;
            }

            action.DisplayName = Language.GetItem(LangItem, "h9");

            var actionEventSource = new ActionEventSource(action);
            actionEventSource.Triggered += (s, e) =>
            {
                var settings = new SettingsWindow();
                var viewModel = new SettingsViewModel(settings);
                settings.DataContext = viewModel;
                settings.Closed += (sender, args) => viewModel.ApplySettings();
                settings.ShowDialog();
            };

            actionEventSources.Add(actionEventSource);

            return action;
        }
        
        /// <summary>
        /// Получить IAction для кнопки запуска Обратной связи
        /// </summary>
        /// <param name="ui">The UI.</param>
        /// <param name="actionEventSources">The action event sources.</param>
        private static IAction GetActionForFeedbackCommand(IUI ui, ICollection<ActionEventSource> actionEventSources)
        {
            var action = ui.CreateAction();

            var extractedImageResource = ExtractResource(Assembly.GetExecutingAssembly(), "mrFeedback_16x16.png");
            if (extractedImageResource != null)
            {
                var icon = ui.CreateImage();
                icon.LoadFromData(extractedImageResource, ImageFormat.ImageFormat_PNG);
                action.Icon = icon;
            }

            action.DisplayName = Language.GetItem("ModPlusAPI", "f1");

            var actionEventSource = new ActionEventSource(action);
            actionEventSource.Triggered += (s, e) =>
            {
                ModPlusAPI.UserInfo.UserInfoService.ShowFeedback();
            };

            actionEventSources.Add(actionEventSource);

            return action;
        }

        /// <summary>
        /// Gets the action for function.
        /// </summary>
        /// <param name="rengaApplication">The renga application.</param>
        /// <param name="actionEventSources">The action event sources.</param>
        /// <param name="loadedFunction">The loaded function.</param>
        /// <param name="isForContextMenu">if set to <c>true</c> [is for context menu].</param>
        /// <param name="isForDropDownMenu">if set to <c>true</c> [is for dropdown menu]</param>
        private static IAction GetActionForFunction(
            IApplication rengaApplication,
            ICollection<ActionEventSource> actionEventSources, 
            LoadedFunction loadedFunction,
            bool isForContextMenu,
            bool isForDropDownMenu)
        {
            var f = GetImplementRengaFunctionFromLoadedFunction(loadedFunction);
            if (f != null)
            {
                var action = rengaApplication.UI.CreateAction();
                var extractedImageResource =
                    isForContextMenu || isForDropDownMenu
                        ? ExtractResource(loadedFunction.PluginAssembly, loadedFunction.Name + "_16x16.png")
                        : ExtractResource(loadedFunction.PluginAssembly, loadedFunction.Name + "_24x24.png");

                if (extractedImageResource != null)
                {
                    var icon = rengaApplication.UI.CreateImage();
                    icon.LoadFromData(extractedImageResource, ImageFormat.ImageFormat_PNG);
                    action.Icon = icon;
                }

                action.DisplayName = isForContextMenu
                    ? "MP:" + loadedFunction.LName
                    : loadedFunction.LName;

                var actionEventSource = new ActionEventSource(action);
                actionEventSource.Triggered += (s, a) =>
                {
                    try
                    {
                        f.Start();
                    }
                    catch (Exception exception)
                    {
                        rengaApplication.UI.ShowMessageBox(
                            MessageIcon.MessageIcon_Error,
                            "ModPlus",
                            $"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                };
                actionEventSources.Add(actionEventSource);

                return action;
            }

            return null;
        }

        private static IAction GetActionForButtonInActionPanel(
            IApplication rengaApplication, List<ActionEventSource> actionEventSources, LoadedFunction loadedFunction)
        {
            var f = GetImplementRengaFunctionFromLoadedFunction(loadedFunction);
            if (f != null)
            {
                var action = rengaApplication.UI.CreateAction();
                var extractedImageResource = ExtractResource(loadedFunction.PluginAssembly, loadedFunction.Name + "_24x24.png");

                if (extractedImageResource != null)
                {
                    var icon = rengaApplication.UI.CreateImage();
                    icon.LoadFromData(extractedImageResource, ImageFormat.ImageFormat_PNG);
                    action.Icon = icon;
                }

                action.ToolTip = loadedFunction.LName;

                var actionEventSource = new ActionEventSource(action);
                actionEventSource.Triggered += (s, a) =>
                {
                    try
                    {
                        f.Start();
                    }
                    catch (Exception exception)
                    {
                        rengaApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message + Environment.NewLine + exception.StackTrace);
                    }
                };
                actionEventSources.Add(actionEventSource);

                return action;
            }

            return null;
        }

        /// <summary>
        /// Вызвать метод Start() для функции, которая не строится в меню
        /// </summary>
        /// <param name="rengaApplication">The renga application.</param>
        /// <param name="loadedFunction">Загруженная функция</param>
        private static void StartFunction(IApplication rengaApplication, LoadedFunction loadedFunction)
        {
            var f = GetImplementRengaFunctionFromLoadedFunction(loadedFunction);
            try
            {
                f.Start();
            }
            catch (Exception exception)
            {
                rengaApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Error, "ModPlus", exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        /// <summary>
        /// Получить реализацию интерфейса IRengaFunction из загруженной функции
        /// </summary>
        /// <param name="loadedFunction">Загруженная функция</param>
        private static IRengaFunction GetImplementRengaFunctionFromLoadedFunction(LoadedFunction loadedFunction)
        {
            foreach (var t in loadedFunction.PluginAssembly.GetTypes())
            {
                var c = t.GetInterface(nameof(IRengaFunction));
                if (c != null && Activator.CreateInstance(t) is IRengaFunction function)
                    return function;
            }

            return null;
        }

        /// <summary>
        /// Extracts the resource.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filename">The filename.</param>
        private static byte[] ExtractResource(Assembly assembly, string filename)
        {
            foreach (var manifestResourceName in assembly.GetManifestResourceNames())
            {
                if (manifestResourceName.Contains(filename))
                {
                    using (var manifestResourceStream = assembly.GetManifestResourceStream(manifestResourceName))
                    {
                        if (manifestResourceStream == null)
                            return null;
                        var ba = new byte[manifestResourceStream.Length];
                        manifestResourceStream.Read(ba, 0, ba.Length);
                        return ba;
                    }
                }
            }

            return null;
        }

        private static ViewType GetRengaViewType(RengaViewType modplusViewType)
        {
            switch (modplusViewType)
            {
                case RengaViewType.Assembly: return ViewType.ViewType_Assembly;
                case RengaViewType.Facade: return ViewType.ViewType_Facade;
                case RengaViewType.Level: return ViewType.ViewType_Level;
                case RengaViewType.ProjectExplorer: return ViewType.ViewType_ProjectExplorer;
                case RengaViewType.Section: return ViewType.ViewType_Section;
                case RengaViewType.SectionProfile: return ViewType.ViewType_SectionProfile;
                case RengaViewType.Sheet: return ViewType.ViewType_Sheet;
                case RengaViewType.Specification: return ViewType.ViewType_Specification;
                case RengaViewType.Table: return ViewType.ViewType_Table;
                case RengaViewType.View3D: return ViewType.ViewType_View3D;
            }

            return ViewType.ViewType_Undefined;
        }
    }
}
