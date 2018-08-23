namespace ModPlus.Enumerators
{
    ////TODO Вынести в интерфейс функции ModPlus

    /// <summary>
    /// Положение кнопки, запускающей функцию
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public enum FunctionUILocation
    {
        /// <summary>
        /// The primary panel
        /// </summary>
        PrimaryPanel,

        /// <summary>
        /// The actionPanel
        /// </summary>
        ActionPanel,

        /// <summary>
        /// The context menu
        /// </summary>
        ContextMenu
    }
}
