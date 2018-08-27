namespace ModPlus.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ModPlusAPI.Interfaces;

    public static class LoadFunctionsHelper
    {
        static LoadFunctionsHelper()
        {
            if (LoadedFunctions == null)
                LoadedFunctions = new List<LoadedFunction>();
        }

        public static List<LoadedFunction> LoadedFunctions { get; set; }

        /// <summary>
        /// Поиск файла функции, если в файле конфигурации вдруг нет атрибута
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <returns>Path to file or empty string</returns>
        public static string FindFile(string functionName)
        {
            var fileName = string.Empty;
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ModPlus");
            using (regKey)
            {
                if (regKey != null)
                {
                    var funcDir = Path.Combine(regKey.GetValue("TopDir").ToString(), "Functions", "Renga", functionName);
                    if (Directory.Exists(funcDir))
                    {
                        foreach (var file in Directory.GetFiles(funcDir, "*.dll", SearchOption.TopDirectoryOnly))
                        {
                            var fileInfo = new FileInfo(file);
                            if (fileInfo.Name.Equals(functionName + ".dll"))
                            {
                                fileName = file;
                                break;
                            }
                        }
                    }
                }
            }

            return fileName;
        }

        public static void GetDataFromFunctionInterface(Assembly loadedFuncAssembly, string fileName)
        {
            var types = GetLoadableTypes(loadedFuncAssembly);
            foreach (var type in types)
            {
                var i = type.GetInterface(typeof(IModPlusFunctionForRenga).Name);
                if (i != null)
                {
                    if (Activator.CreateInstance(type) is IModPlusFunctionForRenga function)
                    {
                        var lf = new LoadedFunction
                        {
                            Name = function.Name,
                            LName = function.LName,
                            ActionButtonViewType = function.ActionButtonViewType,
                            RengaProduct = function.RengaProduct,
                            UiLocation = function.UiLocation,
                            IsAddingToMenuBySelf = function.IsAddingToMenuBySelf,
                            Description = function.Description,
                            SmallIconUrl = "pack://application:,,,/" + loadedFuncAssembly.GetName().FullName +
                                           ";component/Resources/" + function.Name +
                                           "_16x16.png",
                            MiddleIconUrl = "pack://application:,,,/" + loadedFuncAssembly.GetName().FullName +
                                           ";component/Resources/" + function.Name +
                                           "_24x24.png",
                            BigIconUrl = "pack://application:,,,/" + loadedFuncAssembly.GetName().FullName +
                                         ";component/Resources/" + function.Name +
                                         "_32x32.png",
                            FullDescription = function.FullDescription,
                            ToolTipHelpImage = !string.IsNullOrEmpty(function.ToolTipHelpImage)
                            ? "pack://application:,,,/" + loadedFuncAssembly.GetName().FullName + ";component/Resources/Help/" + function.ToolTipHelpImage
                            : string.Empty
                        };

                        LoadedFunctions.Add(lf);
                    }

                    break;
                }
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
