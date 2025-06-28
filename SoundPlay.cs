using System;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace LostGraceSoundPlugin;

public static class SoundPlay
{
    //Gets random file in given directory with a specific extension
    public static string GetRandomFile(string soundsDir, ManualLogSource Logger)
    {
        string filePath = null;
        if (!string.IsNullOrEmpty(soundsDir))
        {
            var extensions = new string[] { ".m4a", ".mp3", ".wav", ".ogg" };
            try
            {
                var dir = new DirectoryInfo(soundsDir);
                var sounds = dir.GetFiles("*.*").Where(f => extensions.Contains(f.Extension.ToLower()));
                System.Random rand = new System.Random();
                filePath = sounds.ElementAt(rand.Next(0, sounds.Count())).FullName;
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to load audio file. Full error: {e}");
            }
        }
        return filePath;
    }

    //Appends AudioType depending on given file extension
    public static AudioType GetAudioType(string filePath, ManualLogSource Logger)
    {
        AudioType fileType = AudioType.UNKNOWN;
        string ext = Path.GetExtension(filePath);
        switch (ext)
        {
            case ".m4a":
                fileType = AudioType.MPEG;
                break;
            case ".mp3":
                fileType = AudioType.MPEG;
                break;
            case ".wav":
                fileType = AudioType.WAV;
                break;
            case ".ogg":
                fileType = AudioType.OGGVORBIS;
                break;
        }
        Logger.LogError($"Unknown file type: {fileType}");
        return fileType;
    }

    //Plays the sound
    public static void PlaySound(string filePath, AudioType fileType, ManualLogSource Logger, Campfire __instance)
    {
        if (__instance.loop && !string.IsNullOrEmpty(filePath))
        {
            //Taken from: https://github.com/susy-bakaa/LCSoundTool/blob/main/Utilities/AudioUtility.cs
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip($"file:///{filePath}", fileType))
            {
                uwr.SendWebRequest();
                // we have to wrap tasks in try/catch, otherwise it will just fail silently
                try
                {
                    while (!uwr.isDone)
                    {

                    }

                    if (uwr.result != UnityWebRequest.Result.Success)
                        Logger.LogError($"Failed to load AudioClip. Full error: {uwr.error}");
                    else
                    {
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