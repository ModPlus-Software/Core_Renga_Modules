namespace ModPlus.Helpers
{
    using System;
    using System.Linq;
    using ModPlusAPI;

    /// <summary>
    /// Дополнительные утилиты локализации в контексте Renga
    /// </summary>
    public static class Localization
    {
        private const string LangItem = "RengaDlls";
        private const string Undef = "undef";

        /// <summary>
        /// Локализованное название типа объекта Renga <see cref="Renga.ObjectTypes"/>
        /// </summary>
        /// <param name="rengaObjectTypeId">Идентификатор типа объекта</param>
        /// <returns>Локализованное значение или значение "Не определено"</returns>
        public static string RengaObjectType(Guid rengaObjectTypeId)
        {
            var name = typeof(Renga.ObjectTypes).GetProperties()
                .FirstOrDefault(p => (Guid)p.GetValue(null) == rengaObjectTypeId)?.Name;

            var value = Language.GetItem(
                LangItem,
                string.IsNullOrEmpty(name) ? nameof(Renga.ObjectTypes.Undefined) : name);

            return string.IsNullOrEmpty(value) ? Language.GetItem(LangItem, Undef) : value;
        }

        /// <summary>
        /// Локализованное название расчетной характеристики <see cref="Renga.QuantityIds"/>
        /// </summary>
        /// <param name="rengaQuantityId">Идентификатор расчетной характеристики</param>
        /// <returns>Локализованное значение или значение "Не определено"</returns>
        public static string RengaQuantity(Guid rengaQuantityId)
        {
            var name = typeof(Renga.QuantityIds).GetProperties()
                .FirstOrDefault(p => (Guid)p.GetValue(null) == rengaQuantityId)?.Name;

            var value = Language.GetItem(LangItem, string.IsNullOrEmpty(name) ? Undef : name);

            return string.IsNullOrEmpty(value) ? Language.GetItem(LangItem, Undef) : value;
        }

        /// <summary>
        /// Локализованное название параметра <see cref="Renga.ParameterIds"/>
        /// </summary>
        /// <param name="rengaParameterId">Идентификатор параметра</param>
        /// <returns>Локализованное значение или значение "Не определено"</returns>
        public static string RengaParameter(Guid rengaParameterId)
        {
            var name = typeof(Renga.ParameterIds).GetProperties()
                .FirstOrDefault(p => (Guid)p.GetValue(null) == rengaParameterId)?.Name;

            var value = Language.GetItem(LangItem, string.IsNullOrEmpty(name) ? Undef : name);

            return string.IsNullOrEmpty(value) ? Language.GetItem(LangItem, Undef) : value;
        }

        /// <summary>
        /// Локализованное название типа данных свойства <see cref="Renga.PropertyType"/>
        /// </summary>
        /// <param name="rengaPropertyType">Тип данных свойства</param>
        /// <returns>Локализованное значение или значение "Не определено"</returns>
        public static string RengaPropertyType(Renga.PropertyType rengaPropertyType)
        {
            var value = Language.GetItem(LangItem, rengaPropertyType.ToString());

            return string.IsNullOrEmpty(value) ? Language.GetItem(LangItem, Undef) : value;
        }

        /// <summary>
        /// Локализованное название логического типа данных <see cref="Renga.Logical"/>
        /// </summary>
        /// <param name="logical">Логический тип данных</param>
        /// <returns>Локализованное значение или значение "Не определено"</returns>
        public static string RengaLogical(Renga.Logical logical)
        {
            var value = Language.GetItem(LangItem, logical.ToString());

            return string.IsNullOrEmpty(value) ? Language.GetItem(LangItem, Undef) : value;
        }

        /// <summary>
        /// Локализованное название типа данных расчетных характеристик <see cref="Renga.QuantityType"/>
        /// </summary>
        /// <param name="rengaQuantityType">Тип данных расчетных характеристик</param>
        /// <returns>Локализованное значение или значение "Не определено"</returns>
        public static string RengaQuantityType(Renga.QuantityType rengaQuantityType)
        {
            var value = Language.GetItem(LangItem, rengaQuantityType.ToString());

            return string.IsNullOrEmpty(value) ? Language.GetItem(LangItem, Undef) : value;
        }
    }
}
