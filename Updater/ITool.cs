/******************************************************************************
* Filename    = ITool.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Interface defined for tool
*****************************************************************************/

namespace Updater;

/// <summary>
/// Interface for tools
/// </summary>
public interface ITool
{
    int Id { get; set; }
    string Description { get; set; }
    Version? Version { get; set; }
    bool IsDeprecated { get; set; }
    string CreatorName { get; set; }
    string CreatorEmail { get; set; }
    Type[] ImplementedInterfaces { get; }
}
