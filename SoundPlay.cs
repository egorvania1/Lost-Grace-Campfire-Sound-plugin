using System;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace LostGraceSoundPlugin;

public static class SoundPlay
{
    //TODO: "Rigged" randomness
    //Current random could play the same file multiple times in a row, and forget about others
    //In my testing, it took 15 tries to get mp3 randomly played (I had 4 valid sound files, all different extensions)

    //TODO: Sync sounds between players
    //Currently, even if all players in lobby will have same sounds in their mod directory,
    //random (most likely) will play different sound for everyone
    
    //Gets random file in given directory with a specific extension
    public static string GetRandomFile(string soundsDir, ManualLogSource Logger)
    {
        //Taken and edited from here: https://stackoverflow.com/a/754504
        string filePath = null;
        if (!string.IsNullOrEmpty(soundsDir))
        {
            var extensions = new string[] { ".mp3", ".aiff", ".aif", ".wav", ".ogg" }; //Allowed types (JPG is not playable audio)
            try
            {
                var dir = new DirectoryInfo(soundsDir);
                var soundFiles = dir.GetFiles("*.*").Where(f => extensions.Contains(f.Extension.ToLower()));
                System.Random rand = new System.Random();
                //I wanted to make seed from system time, but it does it itself actually. Cool.
                filePath = soundFiles.ElementAt(rand.Next(0, soundFiles.Count())).FullName;
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
        if (!string.IsNullOrEmpty(filePath))
        {
            string ext = Path.GetExtension(filePath);
            switch (ext)
            {
                case ".mp3":
                    fileType = AudioType.MPEG;
                    break;
                case ".aiff":
                case ".aif":
                    fileType = AudioType.AIFF;
                    break;
                case ".wav":
                    fileType = AudioType.WAV;
                    break;
                case ".ogg":
                    fileType = AudioType.OGGVORBIS;
                    break;
                default: //Shouldn't happen
                    Logger.LogError($"Unknown file type: {ext}");
                    break;
            }
        }
        return fileType;
    }

    //Plays the sound
    public static void PlaySound(string filePath, AudioType fileType, ManualLogSource Logger, Campfire __instance)
    {
        //Only try to play sound if we have file path, we know file type and for some internal game reason (just in case)
        if (__instance.loop && !string.IsNullOrEmpty(filePath) && fileType != AudioType.UNKNOWN)
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