using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;


namespace LostGraceCampfirePlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static string folderPath;

    [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
    private class Campfire_AddSound_Patch
    {
        static void Prefix(Campfire __instance)
        {
            if (__instance.loop && !string.IsNullOrEmpty(folderPath))
            {
                //Taken from: https://github.com/susy-bakaa/LCSoundTool/blob/main/Utilities/AudioUtility.cs
                AudioClip clip = null;
                using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip($"file:///{folderPath}/lostgrace.mp3", AudioType.MPEG))
                {
                    uwr.SendWebRequest();
                    // we have to wrap tasks in try/catch, otherwise it will just fail silently
                    try
                    {
                        while (!uwr.isDone)
                        {

                        }

                        if (uwr.result != UnityWebRequest.Result.Success)
                            Logger.LogError($"Failed to load WAV AudioClip. Full error: {uwr.error}");
                        else
                        {
                            //YES YES PLEASE PLAY THIS SOUND IM BEGGING YOU
                            clip = DownloadHandlerAudioClip.GetContent(uwr);
                            __instance.loop.PlayOneShot(clip, 10);
                            Logger.LogInfo("Playing campfire sound");
                        }
                    }
                    catch (System.Exception err)
                    {
                        Logger.LogError($"{err.Message}, {err.StackTrace}");
                    }
                }
            }
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
        }
        
        var harmony = new Harmony("com.egorvania1.lostgracesound.patch");
        harmony.PatchAll();

        
    }
}
