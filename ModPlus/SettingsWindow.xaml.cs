namespace ModPlus
{
    using ModPlusStyle.Controls;

    public partial class SettingsWindow : ModPlusWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem("RengaDlls", "h1");
        }
    }
}
