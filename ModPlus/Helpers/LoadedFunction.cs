namespace ModPlus.Helpers
{
    using System.Reflection;
    using ModPlusAPI.Interfaces;

    internal class LoadedFunction : IModPlusFunctionForRenga
    {
        public SupportedProduct SupportedProduct { get; set; }

        public string Name { get; set; }

        public RengaProduct RengaProduct { get; set; }

        public FunctionUILocation UiLocation { get; set; }

        public ContextMenuShowCase ContextMenuShowCase { get; set; }

        public ViewType ViewType { get; set; }

        public string ActionButtonViewType { get; set; }

        public bool IsAddingToMenuBySelf { get; set; }

        public string LName { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string Price { get; set; }

        public string FullDescription { get; set; }

        public Assembly FunctionAssembly { get; set; }
    }
}
