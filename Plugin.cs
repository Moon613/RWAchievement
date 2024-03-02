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
using UnityEngine;
using System.Reflection;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace AchievementMenu;

[BepInPlugin(MOD_ID, "Rain World Achievements", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static ProcessManager.ProcessID AchievementMenu => new ProcessManager.ProcessID("AchievementMenu", true);
    public static ConditionalWeakTable<RainWorld, List<Achievement>> achievements = new();
    public static ManualLogSource? logger;
    static bool postInit = false;
    const string MOD_ID = "moon.achievements";
    const string ACHIEVEMENT_FOLDER = "achievements";
    const char DICTIONARY_SEPARATOR = '~';
    const char SAVE_DATA_SEPARATOR = '|';
    static string unlockDataPath = "";
    public void OnEnable()
    {
        logger = base.Logger;
        // This hook does the achievement loading
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorld.ctor += RainWorld_ctor;
        MenuHooks.Apply();
    }
    private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (!postInit && achievements.TryGetValue(self, out List<Achievement> achievementList)) {
            unlockDataPath = ModManager.ActiveMods.First(x => x.id == MOD_ID).path + Path.DirectorySeparatorChar + "UnlockData.txt";

            #region LoadSteamAchievements
            Debug.Log($"Achievement Mod, there are {SteamUserStats.GetNumAchievements()} Steam achievements");
            for (uint i = 0; i < SteamUserStats.GetNumAchievements(); i++) {
                string achiInternalName = SteamUserStats.GetAchievementName(i);

                // Get and conver time
                SteamUserStats.GetAchievementAndUnlockTime(achiInternalName, out bool unlocked, out uint t);
                string time = Achievement.ConvertTime(t);

                // Get name and description
                string achiName = SteamUserStats.GetAchievementDisplayAttribute(achiInternalName, "name");
                string achiDesc = SteamUserStats.GetAchievementDisplayAttribute(achiInternalName, "desc");
                
                // Getting the sprite for it
                int iconID = SteamUserStats.GetAchievementIcon(achiInternalName);
                if (!Futile.atlasManager.DoesContainElementWithName(achiInternalName) && Achievement.GetSteamIcon(iconID, out Texture2D? texture)) {
                    Futile.atlasManager.LoadAtlasFromTexture(achiInternalName, texture, false);
                }

                if (unlocked) {
                    achievementList.Add(new Achievement(achiName, time, "", achiInternalName, achiDesc, "Steam"));
                }
                else {
                    achievementList.Add(new Achievement("???", "?", "", "multiplayerportrait02", "???", "Steam"));
                }
            }
            #endregion


            #region LoadCustomAchievements
            Achievement.LoadUnlockData();
            string[] files = AssetManager.ListDirectory(ACHIEVEMENT_FOLDER, false, true).Where(file => file.EndsWith(".json")).ToArray();
            for (int i = 0; i < files.Count(); i++) {
                if (File.Exists(files[i])) {
                    files[i] = files[i].Replace('/', Path.DirectorySeparatorChar);
                    try {
                        JObject jobject = JObject.Parse(File.ReadAllText(files[i]));
                        Achievement? achi = jobject.ToObject<Achievement>();
                        if (achi is not null) {
                            Debug.Log($"Achievement Mod {achi} was not null");
                            if (Achievement.dictionary.Keys.FirstOrDefault(x => x == achi.achievementName) == default) {
                                Achievement.dictionary.Add(achi.achievementName, "false");
                                achi.unlocked = false;
                            }
                            else {
                                achi.unlocked = (Achievement.dictionary[achi.achievementName] == "true") ? true : false;
                            }
                            foreach (FieldInfo field in typeof(Achievement).GetFields()) {
                                if (field.Name == nameof(Achievement.dictionary)) {
                                    continue;
                                }
                                // Ensure no null fields
                                if (field.Name != nameof(Achievement.originMod) && field.GetValue(achi) == null) {
                                    Debug.LogWarning($"Value for {field.Name} is null in achievement {files[i].Substring(files[i].IndexOf(ACHIEVEMENT_FOLDER) + ACHIEVEMENT_FOLDER.Length + 1)}");
                                    field.SetValue(achi, "");
                                }
                            }
                            Debug.Log($"Achievement Mod, add achievement: {achi}");
                            achievementList.Add(achi);
                        }
                    }
                    catch (Exception err) {
                        Debug.Log($"Achievements Mod error loading achievements from file!\n{files[i]}\n{err}");
                    }
                }
            }
            Achievement.SaveUnlockData();
            #endregion
            postInit = true;
        }
    }
    private static void RainWorld_ctor(On.RainWorld.orig_ctor orig, RainWorld self)
    {
        orig(self);
        achievements.Add(self, new List<Achievement>());
        achievements.TryGetValue(self, out List<Achievement> achievementList);
    }
    public class Achievement
    {
        public Achievement(string achievementName, string dateAchieved, string imageFolder, string imageName, string description, string originMod)
        {
            this.achievementName = achievementName;
            this.dateAchieved = dateAchieved;
            this.imageFolder = imageFolder;
            this.imageName = imageName;
            this.description = description;
            this.originMod = originMod;
        }
        public static void TriggerAchievement(string achievementName)
        {
            dictionary[achievementName] = "true";
            SaveUnlockData();
        }
        public static void SaveUnlockData()
        {
            string save = "";
            foreach (KeyValuePair<string, string> keyValuePair in dictionary) {
                save += keyValuePair.Key + DICTIONARY_SEPARATOR + keyValuePair.Value + SAVE_DATA_SEPARATOR;
            }
            File.WriteAllText(unlockDataPath, save);
        }
        public static void LoadUnlockData()
        {
            Debug.Log("Achievement Mod loading unlock data");
            string[] unlockSaveData = File.ReadAllText(unlockDataPath).Replace(Environment.NewLine, string.Empty).Trim('|').Split(SAVE_DATA_SEPARATOR);
            if (unlockSaveData.Length == 1 && unlockSaveData[0] == "") {
                Debug.Log("Achievement Mod, no unlock data to load");
                return;
            }
            foreach (string unlockData in unlockSaveData) {
                dictionary.Add(unlockData.Split(DICTIONARY_SEPARATOR)[0], unlockData.Split(DICTIONARY_SEPARATOR)[1]);
            }
            Debug.Log("Achievement Mod loaded unlock data");
        }
        public static string ConvertTime(uint time)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(time).ToLocalTime();
            return dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" + dateTime.Year.ToString();
        }
		public static bool GetSteamIcon(int iconID, out Texture2D? texture)
		{
            texture = null;
            bool result;
            if (!SteamUtils.GetImageSize(iconID, out uint iconWidth, out uint iconHeight)) {
				result = false;
			}
			else {
				byte[] iconBuffer = new byte[iconWidth * iconHeight * 4U];
				if (!SteamUtils.GetImageRGBA(iconID, iconBuffer, iconBuffer.Length)) {
					result = false;
				}
				else {
					texture = new Texture2D((int)iconWidth, (int)iconHeight, TextureFormat.RGBA32, false, true);
					texture.LoadRawTextureData(iconBuffer);
					FlipTextureVertically(texture);
					texture.Apply();
					result = true;
				}
			}
			return result;
        }
		public static void FlipTextureVertically(Texture2D original)
		{
			Color[] originalPixels = original.GetPixels();
			Color[] newPixels = new Color[originalPixels.Length];
			int width = original.width;
			int rows = original.height;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < rows; y++)
				{
					newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
				}
			}
			original.SetPixels(newPixels);
		}
        public override string ToString()
        {
            string ret = "Achievement, ";
            ret += $"{nameof(achievementName)}: {achievementName}, ";
            ret += $"{nameof(dateAchieved)}: {dateAchieved}, ";
            ret += $"{nameof(imageFolder)}: {imageFolder}, ";
            ret += $"{nameof(imageName)}: {imageName}, ";
            ret += $"{nameof(description)}: \"{description.Replace(Environment.NewLine, " ")}\", ";
            ret += $"{nameof(originMod)}: {originMod ?? "NULL"}";
            return ret;
            // string ret = "Achievement, ";
            // foreach (FieldInfo field in typeof(Achievement).GetFields()) {
            //     string s = $"{field.Name}: {field.GetValue(this)}, ";
            //     if (field.Name == nameof(description)) {
            //         s = "\"" + s.Replace('\n', ' ').Replace('\t', ' ') + "\"";
            //     }
            //     if (field.Name == nameof(originMod) && originMod == null) {
            //         s = s.Substring(0, s.Length-2) + "NULL";
            //     }
            //     ret += s;
            // }
            // return ret;
        }
        public static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        public string achievementName = "";
        public string dateAchieved = "";
        public string imageFolder = "";
        public string imageName = "";
        public string description = "";
        public string originMod = "";
        public bool unlocked = false;
    }
}
