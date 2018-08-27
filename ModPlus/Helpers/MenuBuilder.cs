using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ModPlus.Helpers
{
    using System.Collections.Generic;
    using Renga;

    public static class MenuBuilder
    {
        public static void Build(Application rengApplication, List<ActionEventSource> actionEventSources)
        {
            // https://stackoverflow.com/a/38748449/4944499
            // ReSharper disable once ObjectCreationAsStatement
            new System.Windows.Application();

            var ui = rengApplication.UI;

            // Create primary drop down menu
            Renga.IUIPanelExtension uiIPanelExtension = ui.CreateUIPanelExtension();
            Renga.IDropDownButton dropDownButton = ui.CreateDropDownButton();
            var uri = new Uri("pack://application:,,,/ModPlus_Renga;component/Resources/ModPlus_24x24.png");
            IImage icon = ui.CreateImage();
            icon.LoadFromData(GetByteArrayFromUri(uri), ImageFormat.ImageFormat_PNG);
            dropDownButton.Icon = icon;

            // DropDownButton has it's own name and icon
            dropDownButton.ToolTip = "ModPlus";

            dropDownButton.AddAction(GetActionForSettingsCommand(ui, actionEventSources));
            ////dropDownButton.AddAction(CreateAction2());
            uiIPanelExtension.AddDropDownButton(dropDownButton);

            // Add controls to the primary panel:
            ui.AddExtensionToPrimaryPanel(uiIPanelExtension);
        }

        private static byte[] GetByteArrayFromUri(Uri uri)
        {
            var info = System.Windows.Application.GetResourceStream(uri);
            var memoryStream = new MemoryStream();
            info?.Stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        private static IAction GetActionForSettingsCommand(Renga.IUI ui, List<ActionEventSource> actionEventSources)
        {
            var action = ui.CreateAction();

            var uri = new Uri("pack://application:,,,/ModPlus_Renga;component/Resources/Settings_16x16.png");
            IImage icon = ui.CreateImage();
            icon.LoadFromData(GetByteArrayFromUri(uri), ImageFormat.ImageFormat_PNG);
            action.Icon = icon;
            action.DisplayName = "Settings";

            var events = new Renga.ActionEventSource(action);
            events.Triggered += (s, e) =>
            {
                ui.ShowMessageBox(Renga.MessageIcon.MessageIcon_Info, "Plugin message", "Settings");
            };

            actionEventSources.Add(events);

            return action;
        }
    }
}
