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
using MenuShader = Menu.MenuDepthIllustration.MenuShader;

#pragma warning disable CS0618
#pragma warning disable IDE0090
#pragma warning disable IDE0290
#pragma warning disable IDE0028
#pragma warning disable IDE0300

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RWAchievements;

[BepInPlugin(MOD_ID, "Rain World Achievements", "1.0.0")]
internal class Plugin : BaseUnityPlugin
{
    /**********************************************
    The format for the set in the dictionary goes as follows:
    - List 1: shaders for each image, first shader goes to first image
    - List 2: idle camera depths, should be set to around the middle of the image depths
    - List 3: the depths of the depth illustrations, the lower the number the more it moves around
    **********************************************/
    private static readonly Dictionary<string, (List<MenuShader>, List<float>, List<float>)> IDToInfo = new(){
        { "The Survivor", 
            (
                new List<MenuShader>(){MenuShader.Basic, MenuShader.Basic, MenuShader.Basic, MenuShader.Basic, MenuShader.Basic},
                new List<float>(){10f, 10.75f, 9.9f},
                new List<float>(){4,10,10,10,9}
            )
        },
        { "A New Friend",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Six Grains of Gravel, Mountains Abound",
            (
                new List<MenuShader>(){MenuShader.Normal, MenuShader.Normal, MenuShader.Overlay, MenuShader.Normal, MenuShader.Normal, MenuShader.LightEdges, MenuShader.LightEdges, MenuShader.LightEdges},
                new List<float>(){5, 4, 4, 4.5f},
                new List<float>(){9, 8.9f, 1, 5.2f, 4.8f, 4.5f, 4, 4, 3.9f}
            )
        },
        { "The Saint",
            (
                new List<MenuShader>(){MenuShader.Basic, MenuShader.Basic, MenuShader.Basic},
                new List<float>(){4f, 4.75f, 3.9f},
                new List<float>(){3, 4, 8}
            )
        },
        { "The Journey",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Ascension",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Monk",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Two Sprouts, Twelve Brackets",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Chieftain",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Nineteen Spades, Endless Reflections",
            (
                new List<MenuShader>(){MenuShader.Lighten, MenuShader.LightEdges, MenuShader.SoftLight, MenuShader.SoftLight, MenuShader.Normal},
                new List<float>(){4.6f, 4.5f, 4.4f},
                new List<float>(){5f, 4.5f, 2.8f, 2.4f, 2f}
            )
        },
        { "Stolen Enlightenment",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Outlaw",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Droplets upon Five Large Droplets",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Dragon Slayer",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "A Bell, Eighteen Amber Beads",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Four Needles under Plentiful Leaves",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Scholar",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Pilgrimage",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Hunter",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Closure",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Migration",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Nomad",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "An Old Friend",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Messenger",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "A Helping Hand",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Friend",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Pilgrim",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Mother",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Cycle",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Wanderer",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Within Time",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "The Martyr",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Champion",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        },
        { "Expedition Leader",
            (
                new List<MenuShader>(){},
                new List<float>(){1,2,3,4},
                new List<float>(){}
            )
        }
    };
    public static readonly ProcessManager.ProcessID AchievementMenuID = new ProcessManager.ProcessID(nameof(AchievementMenuID), true);
    public static ConditionalWeakTable<RainWorld, List<Achievement>> achievementCWT = new();
    internal static ManualLogSource? logger;
    public static FContainer achievementPopupContainer = new FContainer();
    public static List<Popup> popupList = new List<Popup>();
    private static bool postInit = false;
    private const string MOD_ID = "moon.achievements";
    internal const string ACHIEVEMENT_FOLDER = "achievements";
    internal const char DICTIONARY_SEPARATOR = '~';
    internal const char SAVE_DATA_SEPARATOR = '|';
    internal const char UNLOCK_AND_DATE_SEPARATOR = '`';
    internal const int POPUP_WIDTH = 240;
    internal const int POPUP_HEIGHT = 95;
    internal static string unlockDataPath = "";
    public void OnEnable()
    {
        logger = base.Logger;
        // This hook does the achievement loading
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorld.ctor += RainWorld_ctor;
        MenuHooks.Apply();
        // These hooks are for updating and drawing the achievements.
        On.MainLoopProcess.GrafUpdate += MainLoopProcess_GrafUpdate;
        On.MainLoopProcess.Update += MainLoopProcess_Update;
    }
    private void MainLoopProcess_Update(On.MainLoopProcess.orig_Update orig, MainLoopProcess self)
    {
        orig(self);
        foreach (Popup popup in popupList) {
            popup.Update();
        }
    }
    private void MainLoopProcess_GrafUpdate(On.MainLoopProcess.orig_GrafUpdate orig, MainLoopProcess self, float timeStacker)
    {
        orig(self, timeStacker);
        foreach (Popup popup in popupList) {
            popup.GrafUpdate(timeStacker);
        }
    }
    private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self) {
        orig(self);
        if (!postInit && achievementCWT.TryGetValue(self, out List<Achievement> achievementList)) {
            // This might be needed to ensure that Steam is active.
            int k = 0;
            while (!SteamUserStats.RequestCurrentStats() && k <= 300) { k++; }
            if (k > 300) {
                Debug.Log("RWAchievements: Steam may not be loaded. This could result in the menu not loading, or the achievements being empty or white.");
            }
            
            Futile.stage.AddChild(achievementPopupContainer);
            var assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("achievementshaders"));
            self.Shaders["AchievementPopup"] = FShader.CreateShader("AchievementShader", assetBundle.LoadAsset<Shader>("Assets/AchievementShader.shader"));
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

                string imageFolder = "";
                string imageName = achiInternalName;
                string[]? images = null;
                // If a custom flat image for the achievement exists, use that instead.
                if (File.Exists(AssetManager.ResolveFilePath("achievementImages" + Path.DirectorySeparatorChar + achiName + ".png"))) {
                    imageFolder = "achievementImages";
                    imageName = achiName;
                }
                // If depth images are found, it uses those instead. It finds them by looking in the directory for public the name of the achievement with _1 appended to it
                if (File.Exists(AssetManager.ResolveFilePath("achievementImages" + Path.DirectorySeparatorChar + "depthIllustrations" + Path.DirectorySeparatorChar + achiName + "_1.png"))) {
                    imageFolder = "achievementImages" + Path.DirectorySeparatorChar + "depthIllustrations";
                    imageName = achiName;
                    string[] arr = AssetManager.ListDirectory(imageFolder, false, true).Where(file => file.Contains(achiName.ToLower())).ToArray();
                    images = new string[arr.Length];
                    for (int j = 0; j < arr.Length; j++) {
                        images[j] = imageName + "_" + (j+1);
                    }
                }
                // If the achievement is unlocked, use all the loaded data, otherwise obfuscate it
                if (unlocked) {
                    achievementList.Add(new Achievement(achiName, time, imageFolder, imageName, achiDesc, "Steam", achiInternalName, images, IDToInfo[achiName].Item1, IDToInfo[achiName].Item2, IDToInfo[achiName].Item3){unlocked=true});
                }
                else {
                    achievementList.Add(new Achievement("???", "?", "", "multiplayerportrait02", "???", "Steam", achiInternalName, null, null, null, null){unlocked=false});
                }
            }
            #endregion


            #region LoadCustomAchievements
            Achievement.LoadUnlockData();
            // Gathers all the achievement json files from other mods
            string[] files = AssetManager.ListDirectory(ACHIEVEMENT_FOLDER, false, true).Where(file => file.EndsWith(".json")).ToArray();
            for (int i = 0; i < files.Length; i++) {
                // Make sure it works on Windows and linux and such
                files[i] = files[i].Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                if (File.Exists(files[i])) {
                    try {
                        // Try to parse the json file.
                        Achievement? achi = Achievement.ParseFromJson(JObject.Parse(File.ReadAllText(files[i])));
                        
                        if (achi is not null) {
                            // Make sure an internalID is specified
                            if (achi.internalID == null) {
                                throw new ArgumentException("Achievement Mod: Please give your achievement an internal ID");
                            }

                            achi.imageFolder = achi.imageFolder.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                            // Make sure the internal id does not contain any characters that would cause a parsing error.
                            char[] invalidChars = new[] {DICTIONARY_SEPARATOR, SAVE_DATA_SEPARATOR, UNLOCK_AND_DATE_SEPARATOR};
                            if (invalidChars.Any(c => achi.internalID.Contains(c))) {
                                throw new IOException($"Achievement Mod: Invalid character in internal ID, invalid chars: {DICTIONARY_SEPARATOR}{SAVE_DATA_SEPARATOR}{UNLOCK_AND_DATE_SEPARATOR}");
                            }
                            /*******************
                            Achievement.saveData == save data that was loaded in in Achievement.LoadUnlockData()
                            achi == New achievement that was loaded from a mod's .json file for an achievement
                            *******************/
                            // Detects if this is a new achievement with no unlock save data, and adds it to the save data is so.
                            if (Achievement.saveData.Keys.FirstOrDefault(x => x == achi.internalID) == default) {
                                Achievement.saveData.Add(achi.internalID, new string[2]{"false", Achievement.ConvertTime(0)});
                                achi.unlocked = false;
                                achi.dateAchieved = Achievement.ConvertTime(0);
                            }
                            // If one is found, set the current Achievement data to what is in the unlock save data
                            else {
                                achi.unlocked = Achievement.saveData[achi.internalID][0] == "true";
                                achi.dateAchieved = Achievement.saveData[achi.internalID][1];
                            }
                            // Make sure that the fields of the achievement are not null.
                            foreach (FieldInfo field in typeof(Achievement).GetFields(BindingFlags.Public)) {
                                // Skip the static dictionary field
                                if (field.Name == nameof(Achievement.saveData)) {
                                    continue;
                                }
                                // Ensure no null fields
                                if (field.Name != nameof(Achievement.originMod) && field.Name != nameof(Achievement.images) && field.GetValue(achi) == null) {
                                    Debug.LogWarning($"Value for {field.Name} is null in achievement {files[i].Substring(files[i].IndexOf(ACHIEVEMENT_FOLDER) + ACHIEVEMENT_FOLDER.Length + 1)}");
                                    field.SetValue(achi, "");
                                }
                            }
                            Debug.Log($"Achievement Mod, add achievement: {achi}");
                            achievementList.Add(achi);
                        }
                        else {
                            throw new NullReferenceException("Null Reference Exception, Achievement Mod could not load from file! Make sure that the file actually exists please.");
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
        achievementCWT.Add(self, new List<Achievement>());
    }
}
public class Achievement
{
    internal Achievement(string achievementName, string dateAchieved, string imageFolder, string flatImageName, string description, string? originMod, string internalID, string[]? images, List<MenuShader>? imageShaders, List<float>? idleDepths, List<float>? imageDepths)
    {
        this.achievementName = achievementName;
        this.dateAchieved = dateAchieved;
        this.imageFolder = imageFolder;
        this.flatImageName = flatImageName;
        this.description = description;
        this.originMod = originMod;
        this.internalID = internalID;
        this.images = images;
        this.imageShaders = imageShaders ?? new List<MenuShader>();
        this.idleDepths = idleDepths ?? new List<float>();
        this.imageDepths = imageDepths ?? new List<float>();
    }
    internal static Achievement? ParseFromJson(JObject? Jobj) {
        if (Jobj == null) {
            return null;
        }
        string? id = (string?)Jobj["id"];
        if (id == null) {
            Debug.LogWarning("[Warning : Rain World Achievements] No id found for an acheivement, could not find id to parse.");
            return null;
        }

        string name = (string?)Jobj["name"] ?? "";
        // If a folder cannot be found, then later when loading "Illustrations" folder will be used instead of "".
        string folder = (string?)Jobj["imageFolder"] ?? "";
        string flat = (string?)Jobj["flat"] ?? "";
        string desc = (string?)Jobj["description"] ?? "";
        // origin is intended to be able to be null. If it is null the achievement won't display what added it, which is an intended feature.
        string? origin = (string?)Jobj["origin"];
        // This may need a default case instead of being null, but the base game should be able to handle null values here.
        List<float>? idleDepths = Jobj["idleDepths"]?.Values<float>().ToList();

        List<string>? depthImages = new();
        List<float>? imageDepths = new();
        List<MenuShader>? imageShaders = new();

        if (Jobj["depthIllustrations"] is JToken jToken) {
            foreach (var depthImageData in jToken.Children()) {
                if ((string?)depthImageData["image"] is string img && (float?)depthImageData["depth"] is float dpth && (string?)depthImageData["shader"] is string shdr) {
                    depthImages.Add(img);
                    imageDepths.Add(dpth);
                    switch (shdr.ToLower()) {
                        default:
                        case "basic":
                            imageShaders.Add(MenuShader.Basic);
                            break;
                        case "normal":
                            imageShaders.Add(MenuShader.Normal);
                            break;
                        case "lighten":
                            imageShaders.Add(MenuShader.Lighten);
                            break;
                        case "lightedges":
                            imageShaders.Add(MenuShader.LightEdges);
                            break;
                        case "rain":
                            imageShaders.Add(MenuShader.Rain);
                            break;
                        case "overlay":
                            imageShaders.Add(MenuShader.Overlay);
                            break;
                        case "softlight":
                            imageShaders.Add(MenuShader.SoftLight);
                            break;
                        case "multiply":
                            imageShaders.Add(MenuShader.Multiply);
                            break;
                    }
                }
                else {
                    Debug.LogWarning($"[Warning : Rain World Achievements] Missing data for a depth illustration in achievement {name}. Failed to parse.");
                }
            }
        }
        else {
            idleDepths = null;
            depthImages = null;
            imageDepths = null;
            imageShaders = null;
        }

        return new Achievement(name, "", folder, flat, desc, origin, id, depthImages?.ToArray(), imageShaders, idleDepths, imageDepths);
    }
    public static void TriggerAchievement<T>(RainWorld rainWorld, string ID) where T : Popup {
        if (Plugin.achievementCWT.TryGetValue(rainWorld, out List<Achievement> achievements) && achievements.FirstOrDefault(x => x.internalID == ID) != default) {
            saveData[ID][0] = "true";
            saveData[ID][1] = DateTime.Today.ToShortDateString();
            SaveUnlockData();
            achievements.First(x => x.internalID == ID).unlocked = true;
            try {
                T popup = (T)Activator.CreateInstance(typeof(T), bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, binder: null, args: new object[]{achievements.First(x => x.internalID == ID)}, culture: null);
                Plugin.popupList.Add(popup);
            } catch (Exception err) {
                Debug.LogError("Error creating an instance of an achievement popup. Good luck.\n"+err);
                throw err;
            }
        }
    }
    public static void TriggerAchievement(RainWorld rainWorld, string ID) {
        TriggerAchievement<DefaultPopup>(rainWorld, ID);
    }
    internal static void SaveUnlockData()
    {
        string save = "";
        foreach (KeyValuePair<string, string[]> keyValuePair in saveData) {
            // This stores the save data in the following format:
            // ID~ifitisunlocked`dateitwasachieved|
            // ID: Simply the id of the achievement
            // ifitisunlocked: This will be either "true" or "false" w/o the quotation
            // dateitwasachieved: The date it was achieved, in the format "day/month/year"
            save += keyValuePair.Key + Plugin.DICTIONARY_SEPARATOR + keyValuePair.Value[0] + Plugin.UNLOCK_AND_DATE_SEPARATOR + keyValuePair.Value[1] + Plugin.SAVE_DATA_SEPARATOR;
        }
        File.WriteAllText(Plugin.unlockDataPath, save);
    }
    internal static void LoadUnlockData()
    {
        Debug.Log("Achievement Mod loading unlock data");
        string[] unlockSaveData = File.ReadAllText(Plugin.unlockDataPath).Replace(Environment.NewLine, string.Empty).Trim('|').Split(Plugin.SAVE_DATA_SEPARATOR);
        if (unlockSaveData.Length == 1 && unlockSaveData[0] == "") {
            Debug.Log("Achievement Mod, no unlock data to load");
            return;
        }
        foreach (string unlockData in unlockSaveData) {
            string data = unlockData.Split(Plugin.DICTIONARY_SEPARATOR)[1];
            saveData.Add(unlockData.Split(Plugin.DICTIONARY_SEPARATOR)[0], new string[2]{data.Split(Plugin.UNLOCK_AND_DATE_SEPARATOR)[0], data.Split(Plugin.UNLOCK_AND_DATE_SEPARATOR)[1]});
        }
        Debug.Log("Achievement Mod loaded unlock data");
    }
    internal static string ConvertTime(uint time)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(time).ToLocalTime();
        return dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" + dateTime.Year.ToString();
    }
    internal static bool GetSteamIcon(int iconID, out Texture2D? texture)
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
    private static void FlipTextureVertically(Texture2D original)
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
        ret += $"{nameof(internalID)}: {internalID}, ";
        ret += $"{nameof(achievementName)}: {achievementName}, ";
        ret += $"{nameof(dateAchieved)}: {dateAchieved}, ";
        ret += $"{nameof(imageFolder)}: {(imageFolder == ""? "Illustrations" : imageFolder)}, ";
        ret += $"{nameof(flatImageName)}: {flatImageName}, ";
        ret += $"{nameof(description)}: \"{description.Replace("\n", " ").Replace("\r", " ").Replace("\r\n", " ")}\", ";
        ret += $"{nameof(originMod)}: {originMod ?? "NULL"}, ";
        ret += $"{nameof(images)}: {(images == null? "NULL" : string.Join(" ", images))}, ";
        ret += $"{nameof(imageShaders)}: {string.Join(" ", imageShaders)}, ";
        ret += $"{nameof(idleDepths)}: {string.Join(" ", idleDepths)}, ";
        ret += $"{nameof(imageDepths)}: {string.Join(" ", imageDepths)}, ";
        return ret;
    }
    internal static Dictionary<string, string[]> saveData = new Dictionary<string, string[]>();
    public string achievementName = "";
    public string dateAchieved = "";
    public string imageFolder = "";
    public string flatImageName = "";
    public string[]? images = null;
    public List<MenuShader> imageShaders = new List<MenuShader>();
    public List<float> idleDepths = new List<float>();
    public List<float> imageDepths = new List<float>();
    public string description = "";
    public string? originMod = null;
    public bool unlocked = false;
    // This MUST be unique
    public string internalID = "";
}
