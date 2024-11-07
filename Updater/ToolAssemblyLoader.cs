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
public class ToolAssemblyLoader
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
    public static Dictionary<string, List<string>> LoadToolsFromFolder(string folder)
    {
        Dictionary<string, List<string>> toolPropertyMap = [];

        try
        {
            // Ensure the folder exists, if not, create it
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Trace.WriteLine($"[Updater] Directory '{folder}' did not exist, created successfully.");
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

                            Type toolInterface = typeof(ITool);
                            Type[] types = assembly.GetTypes();

                            foreach (Type type in types)
                            {

                                // only non abstract classes implementing ITool should be fetched
                                if (toolInterface.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                                {
                                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                                    // Attempt to create an instance of the type that implements ITool
                                    try
                                    {
                                        object? instance = Activator.CreateInstance(type);
                                        if (instance != null)
                                        {
                                            Trace.WriteLine($"[Updater] Instance of {type.FullName} created successfully!");

                                            PropertyInfo[] properties = toolInterface.GetProperties();
                                            foreach (PropertyInfo property in properties)
                                            {
                                                if (property.CanRead)  // To ensure the property is readable
                                                {
                                                    Trace.WriteLine(property.Name);
                                                    object? value = property.GetValue(instance);

                                                    string valueString = value is Version version ? version.ToString() : $"{value}";

                                                    if (toolPropertyMap.TryGetValue(property.Name, out List<string>? values))
                                                    {
                                                        values.Add(valueString); // Append to the existing list if the key exists
                                                    }
                                                    else
                                                    {
                                                        toolPropertyMap[property.Name] = [valueString]; // Create a new list for the new key
                                                    }

                                                }
                                            }
                                            Trace.WriteLine("[Updater] Successfully read all properties");
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
            Trace.WriteLine($"Unexpected error: {ex.Message}");
        }

        Trace.WriteLine("[Updater] Successfully created map with tool properties");
        return toolPropertyMap;
    }
}
