namespace ModPlus.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ModPlusAPI;
    using ModPlusAPI.Interfaces;

    internal static class LoadFunctionsHelper
    {
        static LoadFunctionsHelper()
        {
            if (LoadedFunctions == null)
                LoadedFunctions = new List<LoadedFunction>();
        }
        
        internal static List<LoadedFunction> LoadedFunctions { get; set; }

        /// <summary>
        /// Поиск файла функции, если в файле конфигурации вдруг нет атрибута
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <returns>Path to file or empty string</returns>
        internal static string FindFile(string functionName)
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

        internal static void GetDataFromFunctionInterface(Assembly loadedFuncAssembly, string fileName)
        {
            var types = GetLoadableTypes(loadedFuncAssembly);
            foreach (var type in types)
            {
                var i = type.GetInterface(nameof(IModPlusFunctionForRenga));
                if (i != null)
                {
                    if (Activator.CreateInstance(type) is IModPlusFunctionForRenga function)
                    {
                        var lf = new LoadedFunction
                        {
                            Name = function.Name,
                            LName = Language.GetFunctionLocalName(function.Name, function.LName),
                            ViewTypes = function.ViewTypes,
                            UiLocation = function.UiLocation,
                            ContextMenuShowCase = function.ContextMenuShowCase,
                            IsAddingToMenuBySelf = function.IsAddingToMenuBySelf,
                            Description = Language.GetFunctionShortDescription(function.Name, function.Description),
                            FullDescription =
                                Language.GetFunctionFullDescription(function.Name, function.FullDescription),
                            FunctionAssembly = loadedFuncAssembly
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
