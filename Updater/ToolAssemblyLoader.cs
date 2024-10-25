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

namespace Updater;

/// <summary>
/// Class to Load information of Tools in a hash map.
/// </summary>
public class ToolAssemblyLoader : IToolAssemblyLoader
{
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
        Dictionary<string, List<string>> toolPropertyMap = new Dictionary<string, List<string>>();

        try
        {
            // Ensure the folder exists, if not, create it
            if (!Directory.Exists(folder))
            {
                Trace.WriteLine($"Directory '{folder}' does not exist. Creating it...");
                Directory.CreateDirectory(folder);
                Trace.WriteLine($"Directory '{folder}' created successfully.");
                return toolPropertyMap; // Exit function if folder is newly created, as it would be empty
            }

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
                                        object? instance = Activator.CreateInstance(type);
                                        if (instance != null)
                                        {
                                            Trace.WriteLine($"Instance of {type.FullName} created successfully!");

                                            PropertyInfo[] properties = toolInterface.GetProperties();
                                            foreach (PropertyInfo property in properties)
                                            {
                                                if (property.CanRead)  // To ensure the property is readable
                                                {
                                                    object? value = property.GetValue(instance);

                                                    if (toolPropertyMap.ContainsKey($"{property.Name}"))
                                                    {
                                                        toolPropertyMap[$"{property.Name}"].Add($"{value}");    // appending to the map values if key exists
                                                    }
                                                    else
                                                    {
                                                        toolPropertyMap[$"{property.Name}"] = new List<string> { $"{value}" };  // creating a new list for values for new key
                                                    }

                                                    Trace.WriteLine($"{property.Name} = {value}");
                                                }
                                            }
                                        }

                                        else
                                        {
                                            throw new InvalidOperationException($"Failed to create instance for {type.FullName}. Constructor might be missing or inaccessible.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new InvalidOperationException($"Failed to create an instance of {type.FullName}: {ex.Message}", ex);
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
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error: {ex.Message}", ex);
        }
        return toolPropertyMap;
    }
}
