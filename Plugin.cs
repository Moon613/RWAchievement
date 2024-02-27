using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using Debug = UnityEngine.Debug;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace AchievementMenu;

[BepInPlugin("moon.achievements", "Mod Name", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static ProcessManager.ProcessID AchievementMenu => new ProcessManager.ProcessID("AchievementMenu", register: true);
    public void OnEnable()
    {
        Logger.LogDebug("Achievements, applying hooks");
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        try {
            MenuHooks.Apply();
        } catch (Exception err) {
            Logger.LogDebug(err);
        }
    }

    private bool IsInit;
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
}
