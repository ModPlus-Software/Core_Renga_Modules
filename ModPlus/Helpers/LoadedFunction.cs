namespace ModPlus.Helpers
{
    using System.Collections.Generic;
    using System.Reflection;
    using ModPlusAPI.Abstractions;
    using ModPlusAPI.Enums;

    /// <summary>
    /// Загруженный плагин
    /// </summary>
    internal class LoadedFunction : IModPlusPluginForRenga
    {
        /// <inheritdoc/>
        public SupportedProduct SupportedProduct { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public RengaFunctionUILocation UiLocation { get; set; }

        /// <inheritdoc/>
        public RengaContextMenuShowCase ContextMenuShowCase { get; set; }

        /// <inheritdoc/>
        public List<RengaViewType> ViewTypes { get; set; }

        /// <inheritdoc/>
        public bool IsAddingToMenuBySelf { get; set; }

        /// <inheritdoc/>
        public string LName { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public string Author { get; set; }

        /// <inheritdoc/>
        public string Price { get; set; }

        /// <inheritdoc/>
        public string FullDescription { get; set; }

        /// <summary>
        /// Plugin assembly
        /// </summary>
        public Assembly PluginAssembly { get; set; }
    }
}
