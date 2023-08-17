using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace SortedCargo;

[BepInPlugin("me.fistme.dave.sortedcargo", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class SortedCargo : BasePlugin
{
    public static ManualLogSource LogSource { get; private set; }
    public static SortedCargoBehaviour Behaviour { get; private set; }

    public static new void Log(string message)
    {
        LogSource.LogInfo(message);
    }

    public static new void Log(LogLevel level, string message)
    {
        LogSource.Log(level, message);
    }

    public override void Load()
    {
        LogSource = base.Log;
        Behaviour = AddComponent<SortedCargoBehaviour>();
    }

    public override bool Unload()
    {
        UnityEngine.Object.DestroyImmediate(Behaviour);
        Behaviour = null;
        return base.Unload();
    }
}
