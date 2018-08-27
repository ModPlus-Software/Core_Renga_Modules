namespace ModPlus.Helpers
{
    using ModPlusAPI.Interfaces;

    public class LoadedFunction : IModPlusFunctionForRenga
    {
        public SupportedProduct SupportedProduct { get; set; }

        public string Name { get; set; }

        public RengaProduct RengaProduct { get; set; }

        public FunctionUILocation UiLocation { get; set; }

        public string ActionButtonViewType { get; set; }

        public bool IsAddingToMenuBySelf { get; set; }

        public string LName { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string Price { get; set; }

        public string FullDescription { get; set; }

        public string ToolTipHelpImage { get; set; }

        public string SmallIconUrl { get; set; }

        public string MiddleIconUrl { get; set; }

        public string BigIconUrl { get; set; }
    }
}
