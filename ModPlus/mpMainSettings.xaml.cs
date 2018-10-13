namespace ModPlus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using ModPlusAPI;
    using ModPlusStyle.Controls;

    public partial class MpMainSettings : ModPlusWindow
    {
        private readonly string _curLang;
        private readonly string _langItem = "RengaDlls";

        public MpMainSettings()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(_langItem, "h1");
            FillThemesAndColors();
            SetAppRegistryKeyForCurrentUser();
            GetDataByVars();
            Closing += MpMainSettings_Closing;
            Closed += MpMainSettings_OnClosed;

            // fill languages
            CbLanguages.ItemsSource = ModPlusAPI.Language.GetLanguagesByFiles();
            CbLanguages.SelectedItem = ((List<Language.LangItem>)CbLanguages.ItemsSource)
                .FirstOrDefault(x => x.Name.Equals(ModPlusAPI.Language.CurrentLanguageName));
            _curLang = ((Language.LangItem)CbLanguages.SelectedItem)?.Name;
            CbLanguages.SelectionChanged += CbLanguages_SelectionChanged;
        }

        // Change language
        private void CbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is Language.LangItem langItem)
            {
                ModPlusAPI.Language.SetCurrentLanguage(langItem);
                this.SetLanguageProviderForModPlusWindow();
            }
        }

        private void FillThemesAndColors()
        {
            MiTheme.ItemsSource = ModPlusStyle.ThemeManager.Themes;
            var pluginStyle = ModPlusStyle.ThemeManager.Themes.First();
            var savedPluginStyleName = Regestry.GetValue("PluginStyle");
            if (!string.IsNullOrEmpty(savedPluginStyleName))
            {
                var theme = ModPlusStyle.ThemeManager.Themes.Single(t => t.Name == savedPluginStyleName);
                if (theme != null)
                    pluginStyle = theme;
            }

            MiTheme.SelectedItem = pluginStyle;
        }

        // Заполнение поля Ключ продукта
        private void SetAppRegistryKeyForCurrentUser()
        {
            ////TbRegistryKey.Text = Variables.RegistryKey;
            ////var regVariant = Regestry.GetValue("RegestryVariant");
            ////if (!string.IsNullOrEmpty(regVariant))
            ////{
            ////    TbAboutRegKey.Visibility = System.Windows.Visibility.Visible;
            ////    if (regVariant.Equals("0"))
            ////    {
            ////        TbAboutRegKey.Text = ModPlusAPI.Language.GetItem(_langItem, "h10") + " " + Regestry.GetValue("HDmodel");}
            ////    else if (regVariant.Equals("1"))
            ////    {
            ////        TbAboutRegKey.Text = ModPlusAPI.Language.GetItem(_langItem, "h11") + " " + Regestry.GetValue("gName");}
            ////}
        }

        /// <summary>
        /// Получение значений из глобальных переменных плагина
        /// </summary>
        private void GetDataByVars()
        {
            ////try
            ////{
            ////    // email
            ////    TbEmailAddress.Text = Variables.UserEmail;
            ////}
            ////catch (Exception exception)
            ////{
            ////    ExceptionBox.Show(exception);
            ////}
        }

        // Выбор темы
        private void MiTheme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = (ModPlusStyle.Theme) e.AddedItems[0];
            Regestry.SetValue("PluginStyle", theme.Name);
            ModPlusStyle.ThemeManager.ChangeTheme(this, theme);
        }

        private void MpMainSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ////if (!string.IsNullOrEmpty(TbEmailAddress.Text))
            ////{
            ////    if (IsValidEmail(TbEmailAddress.Text))
            ////    {
            ////        TbEmailAddress.BorderBrush = FindResource("BlackBrush") as Brush;
            ////    }
            ////    else
            ////    {
            ////        TbEmailAddress.BorderBrush = Brushes.Red;
            ////        ModPlusAPI.Windows.MessageBox.Show(ModPlusAPI.Language.GetItem(_langItem, "tt4"));
            ////        TbEmailAddress.Focus();
            ////        e.Cancel = true;
            ////    }
            ////}
        }

        private void MpMainSettings_OnClosed(object sender, EventArgs e)
        {
            ////try
            ////{
            ////    // Сохраняем в реестр почту, если изменилась
            ////    Variables.UserEmail = TbEmailAddress.Text;
            ////    if (!((Language.LangItem)CbLanguages.SelectedItem).Name.Equals(_curLang))
            ////    {
            ////        ModPlusAPI.Windows.MessageBox.Show(ModPlusAPI.Language.GetItem(_langItem, "tt5"));
            ////    }
            ////}
            ////catch (Exception ex)
            ////{
            ////    ExceptionBox.Show(ex);
            ////}
        }

        private void TbEmailAddress_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (IsValidEmail(tb.Text))
                    tb.BorderBrush = FindResource("BlackBrush") as Brush;
                else
                    tb.BorderBrush = Brushes.Red;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
