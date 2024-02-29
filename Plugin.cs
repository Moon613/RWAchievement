using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;
using Steamworks;
using BepInEx.Logging;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace AchievementMenu;

[BepInPlugin("moon.achievements", "Rain World Achievements", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static ProcessManager.ProcessID AchievementMenu => new ProcessManager.ProcessID("AchievementMenu", true);
    public static ConditionalWeakTable<RainWorld, List<Achievement>> achievements = new();
    public static ManualLogSource logger;
    static bool postInitTriggered = false;
    private static bool IsInit;
    public void OnEnable()
    {
        logger = base.Logger;
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorld.ctor += RainWorld_ctor;
        MenuHooks.Apply();
    }
    private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (!postInitTriggered && achievements.TryGetValue(self, out List<Achievement> achievementList)) {

            #region LoadSteamAchievements
            Debug.Log($"Achievement Mod, there are {SteamUserStats.GetNumAchievements()} Steam achievements");
            for (uint i = 0; i < SteamUserStats.GetNumAchievements(); i++) {
                string achiInternalName = SteamUserStats.GetAchievementName(i);
                SteamUserStats.GetAchievementAndUnlockTime(achiInternalName, out bool unlocked, out uint time);
                string achiName = SteamUserStats.GetAchievementDisplayAttribute(achiInternalName, "name");
                string achiDesc = SteamUserStats.GetAchievementDisplayAttribute(achiInternalName, "desc");
                
                // Getting the sprite for it
                int iconID = SteamUserStats.GetAchievementIcon(achiInternalName);

                Debug.Log($"Achievement Mod: {achiInternalName}, {achiName}, {time}, {achiDesc}, {unlocked}");
                if (unlocked) {
                    achievementList.Add(new Achievement(achiName, time.ToString(), "", "basegame_thumbnail", achiDesc));
                }
                else {
                    achievementList.Add(new Achievement("???", "locked", "", "multiplayerportrait02", "???"));
                }
            }
            #endregion


            #region LoadCustomAchievements
            string[] files = AssetManager.ListDirectory("achievements", false, true).Where(file => file.EndsWith(".json")).ToArray();
            for (int i = 0; i < files.Count(); i++) {
                if (File.Exists(files[i])) {
                    files[i] = files[i].Replace('/', Path.DirectorySeparatorChar);
                    try {
                        JObject jobject = JObject.Parse(File.ReadAllText(files[i]));
                        Achievement? achi = jobject.ToObject<Achievement>();
                        if (achi is not null) {
                            achievementList.Add(achi);
                        }
                    }
                    catch (Exception err) {
                        Debug.Log($"Achievements Mod error loading achievements from file!\n{files[i]}\n{err}");
                    }
                }
            }
            #endregion
            postInitTriggered = true;
        }
    }
    private static void RainWorld_ctor(On.RainWorld.orig_ctor orig, RainWorld self)
    {
        orig(self);
        achievements.Add(self, new List<Achievement>());
        achievements.TryGetValue(self, out List<Achievement> achievementList);
    }
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;

            
            IsInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    public class Achievement
    {
        public Achievement(string achievementName, string dateAchieved, string imageFolder, string imageName, string description)
        {
            this.achievementName = achievementName;
            this.dateAchieved = dateAchieved;
            this.imageFolder = imageFolder;
            this.imageName = imageName;
            this.description = description;
        }
        public string achievementName = "";
        public string dateAchieved = "";
        public string imageFolder = "";
        public string imageName = "";
        public string description = "";
}
}
