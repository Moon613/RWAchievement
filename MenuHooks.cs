﻿using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Debug = UnityEngine.Debug;
using Vector2 = UnityEngine.Vector2;

namespace RWAchievements;
internal class MenuHooks {
    public static void Apply() {
        On.Menu.MainMenu.ctor += MainMenu_ctor;
        IL.Menu.MainMenu.AddMainMenuButton += IL_MainMenu_AddMainMenuButton;
        IL.ProcessManager.PostSwitchMainProcess += IL_ProcessManager_PostSwitchMainProcess;
    }
    private static void IL_ProcessManager_PostSwitchMainProcess(ILContext il) {
        var cursor = new ILCursor(il);

        try {
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                                        i => i.MatchLdfld<ProcessManager>("oldProcess"),
                                                        i => i.MatchLdarg(0),
                                                        i => i.MatchLdfld<ProcessManager>("currentMainLoop"),
                                                        i => i.MatchCallOrCallvirt<MainLoopProcess>("CommunicateWithUpcomingProcess")))
            {
                throw new Exception("Failed to match IL for ProcessManager_PostSwitchMainProcess!");
            }
        }
        catch (Exception ex) {
            Debug.LogError("Exception when matching IL for ProcessManager_PostSwitchMainProcess!");
            Debug.LogException(ex);
            Debug.LogError(il);
            throw;
        }

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate((ProcessManager self, ProcessManager.ProcessID ID) =>
        {
            if (ID == Plugin.AchievementMenuID)
            {
                self.currentMainLoop = new AchievementMenu(self);
            }
        });

    }

    private static void IL_MainMenu_AddMainMenuButton(ILContext il)
    {
        var cursor = new ILCursor(il);

        try
        {
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(8)))
            {
                throw new Exception("Failed to match IL for MainMenu_ctor1!");
            }

            cursor.MoveAfterLabels();
            cursor.EmitDelegate((int _) => 12);
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception when matching IL for MainMenu_ctor1!");
            Debug.LogException(ex);
            Debug.LogError(il);
            throw;
        }
    }

    private static void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
    {
        orig(self, manager, showRegionSpecificBkg);
        Debug.Log("HAHAH it is coding indeed");

        float buttonWidth = Menu.MainMenu.GetButtonWidth(self.CurrLang);
        Vector2 pos = new Vector2(683f - buttonWidth / 2f, 0f);
        Vector2 size = new Vector2(buttonWidth, 30f);
        
        self.AddMainMenuButton(new SimpleButton(self, self.pages[0], self.Translate("Achievements".ToUpper()), "AAAAAAAA", pos, size), () => {
            manager.RequestMainProcessSwitch(Plugin.AchievementMenuID);
            manager.musicPlayer.FadeOutAllSongs(180);
            manager.menuMic.AllQuiet();
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }, ModManager.MSC? 2 : 3);
    }
}