using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;


namespace LostGraceSoundPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static string folderPath;

    [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
    private class Campfire_AddSound_Patch
    {
        static void Prefix(Campfire __instance)
        {
            string filePath = SoundPlay.GetRandomFile(folderPath, Logger); //Get random file
            AudioType fileType = SoundPlay.GetAudioType(filePath, Logger); //Get it's AudioType
            SoundPlay.PlaySound(filePath, fileType, Logger, __instance);   //Play audio
        }
    }
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        List<string> bonfireSoundPaths = Directory.GetDirectories(Paths.PluginPath, "lostgracesoundplugin", SearchOption.AllDirectories).ToList<string>();
        if (!bonfireSoundPaths.Any())
        {
            Logger.LogError("Couldn't find lostgracesoundplugin folder");
        }
        else
        {
            folderPath = bonfireSoundPaths[0];
            Logger.LogDebug($"{folderPath} is the campfire sound path.");
            var harmony = new Harmony("com.egorvania1.lostgracesound.patch");
            harmony.PatchAll();
        }
    }
}
