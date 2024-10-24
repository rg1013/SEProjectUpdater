/******************************************************************************
* Filename    = ToolAssemblyLoader.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Loads information of ITool from a give folder path
*****************************************************************************/


using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using ActivityTracker;


namespace Updater;

/// <summary>
/// Class to Load information of Tools in a hash map.
/// </summary>
public class ToolAssemblyLoader : IToolAssemblyLoader
{
    private readonly ILogService _logService = new LogService();

    /// <summary>
    /// Checks if a file is a dll file or not
    /// </summary>
    /// <param name="path">Path to the .NET assembly.</param>
    static bool IsDLLFile(string path)
    {
        return Path.GetExtension(path).Equals(".dll", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns hash map of information of tools.
    /// </summary>
    /// <param name="folder">Path to the target folder</param>
    public Dictionary<string, List<string>> LoadToolsFromFolder(string folder)
    {
        Dictionary<string, List<string>> hashMap = new Dictionary<string, List<string>>();

        try
        {
            _logService.LogMessage("Here");
            string[] files = Directory.GetFiles(folder);

            foreach (string file in files)
            {
                // only processing dll files
                if (File.Exists(file) && IsDLLFile(file))
                {
                    Assembly fileAssembly = Assembly.LoadFile(file);

                    TargetFrameworkAttribute? targetFrameworkAttribute = fileAssembly.GetCustomAttribute<TargetFrameworkAttribute>();

                    // the tools are limited to .NET version 8.0
                    if (targetFrameworkAttribute != null && targetFrameworkAttribute.FrameworkName == ".NETCoreApp,Version=v8.0")
                    {
                        try
                        {
                            Assembly assembly = Assembly.LoadFrom(file);
                            Trace.WriteLine($"Assembly: {assembly.FullName}");

                            Type toolInterface = typeof(ITool);
                            Type[] types = assembly.GetTypes();

                            foreach (Type type in types)
                            {

                                // only classes implementing ITool should be fetched
                                if (toolInterface.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                                {
                                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                                    // Attempt to create an instance of the type that implements ITool
                                    try
                                    {
                                        object instance = Activator.CreateInstance(type);
                                        Trace.WriteLine($"Instance of {type.FullName} created successfully!");


                                        PropertyInfo[] properties = toolInterface.GetProperties();
                                        foreach (PropertyInfo property in properties)
                                        {
                                            if (property.CanRead)  // To ensure the property is readable
                                            {
                                                object value = property.GetValue(instance);
                                                if (hashMap.ContainsKey($"{property.Name}"))
                                                {
                                                    hashMap[$"{property.Name}"].Add($"{value}");
                                                }
                                                else
                                                {
                                                    hashMap[$"{property.Name}"] = new List<string> { $"{value}" };
                                                }

                                                Console.WriteLine($"{property.Name} = {value}");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Trace.WriteLine($"Failed to create an instance of {type.FullName}: {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"Error while processing {file}: {e.Message}");
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"Invalid Target Framework for Assembly {fileAssembly.GetName()}.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Error in Main: {e.Message}");

        }

        return hashMap;
    }
}
