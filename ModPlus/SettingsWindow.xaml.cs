namespace ModPlus
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem("RengaDlls", "h1");
            ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources, "LangApi");
        }
    }
}
