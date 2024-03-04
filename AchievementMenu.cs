using System.Collections.Generic;
using Menu;
using UnityEngine;
using static AchievementMenu.Plugin;

namespace AchievementMenu;
public class AchievementMenu : Menu.Menu
{
    public int currentSelectedPage = 0;
    float screenWidth => manager.rainWorld.options.ScreenSize.x;
    float screenHeight => manager.rainWorld.options.ScreenSize.y;
    Vector2 screenCenter => manager.rainWorld.options.ScreenSize/2f;
    float movementPerStep;
    const int STEPS = 20;
    int stepsTaken;
    public BigArrowButton prevButton;
    public BigArrowButton nextButton;
    public AchievementMenu(ProcessManager processManager) : base(processManager, Plugin.AchievementMenu) {
        mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
        currentPage = 0;
        stepsTaken = STEPS;
        movementPerStep = screenWidth/STEPS;

        #region Page 1
        pages.Add(new Page(this, null, "main", 0){pos=new Vector2(1366, 768) - manager.rainWorld.options.ScreenSize});
        pages[0].subObjects.Add(new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.Empty));
        
        const int BigArrowButtonWidth = 25;
        Vector2 adjustForPageOffsetDueToResolution = 0.5f*pages[0].pos;

        // The next page button
        nextButton = new BigArrowButton(this, pages[0], "NEXT", new Vector2((screenWidth/2f) + 200f - BigArrowButtonWidth, 50f) - adjustForPageOffsetDueToResolution, 1);
        pages[0].subObjects.Add(nextButton);

        // The prev page button
        prevButton = new BigArrowButton(this, pages[0], "PREV", new Vector2((screenWidth/2f) - 200f - BigArrowButtonWidth, 50f) - adjustForPageOffsetDueToResolution, -1);
        pages[0].subObjects.Add(prevButton);

        // The back button lol
        const int BackButtonWidth = 160;
        SimpleButton backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(screenCenter.x-(BackButtonWidth/2), 50f) - adjustForPageOffsetDueToResolution, new Vector2(BackButtonWidth, 50f));
        pages[0].subObjects.Add(backButton);
        backObject = backButton;
        backButton.nextSelectable[0] = prevButton;
        backButton.nextSelectable[2] = nextButton;

        // List view button
        SimpleButton listView = new SimpleButton(this, pages[0], "A", "LIST", new Vector2(-0.8f*screenWidth, 0.8f*screenHeight) - adjustForPageOffsetDueToResolution, new Vector2(50, 50));
        pages[0].subObjects.Add(listView);
        #endregion

        pages.Add(new Page(this, null, "achipages", 1){pos=new Vector2(1366, 768) - manager.rainWorld.options.ScreenSize});
        adjustForPageOffsetDueToResolution = 0.5f*pages[1].pos;
        // Removes the mouseCursor from the subObjects.
        pages[1].subObjects.Clear();
        
        if (achievements.TryGetValue(manager.rainWorld, out List<Achievement> achievementList)) {
            for (int i = 0; i < achievementList.Count; i++) {
                Achievement achievement = achievementList[i];
                // Debug.Log($"Achievement Mod: {achievement.achievementName}, {achievement.imageFolder}, {achievement.imageName}, {achievement.description}");
                pages[1].subObjects.Add(new AchievementPage(this, pages[1], achievement.achievementName, i, new Vector2(screenWidth/2f + (i-2) * screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, achievement));
            }
        }

        // pages[1].subObjects.Add(new AchievementPage(this, pages[1], "ach1", 2, new Vector2(screenWidth/2f + 2*screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, "", "aidesktopimg", "ACHIEVEMENT NAME", "2/27/2024", "ACHIEVEMENT\nDESCRIPTION"));
        
        // pages[1].subObjects.Add(new AchievementPage(this, pages[1], "ach2", 3, new Vector2((screenWidth/2f) + 3*screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, "", "full_figure_red", "ACHIEVEMENT NAME 2", "2/27/2024", "ACHIEVEMENT DESCRIPTION\n2"));
    }
    public override void Update()
    {
        base.Update();
        if (Input.GetKey(KeyCode.Escape))
        {
            Singal(backObject, "BACK");
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            Singal(pages[0], "PREV");
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            Singal(pages[0], "NEXT");
        }
        if (stepsTaken < STEPS) {
            foreach (AchievementPage page in pages[1].subObjects) {
                if (page.pos.x >= ((pages[1].subObjects.Count-2)*screenWidth+screenWidth/2f)) {
                    page.pos.x = -screenWidth-screenWidth/2f;
                    page.lastPos = page.pos;
                }
                page.pos.x += movementPerStep;
            }
            stepsTaken++;
        }
        if (stepsTaken > STEPS) {
            foreach (AchievementPage page in pages[1].subObjects) {
                if (page.pos.x <= (-screenWidth-screenWidth/2f)) {
                    page.pos.x = (screenWidth/2f) + ((pages[1].subObjects.Count-2)*screenWidth);
                    page.lastPos = page.pos;
                }
                page.pos.x -= movementPerStep;
            }
            stepsTaken--;
        }
        if (stepsTaken == STEPS) {
            nextButton.inactive = false;
            prevButton.inactive = false;
        }
    }
    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if (message == "BACK") {
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            PlaySound(SoundID.MENU_Switch_Page_Out);
        }
        if (message == "NEXT" && stepsTaken == STEPS) {
            PlaySound(SoundID.MENU_Next_Slugcat, 0, 1.4f, 0.4f);
            currentSelectedPage++;
            stepsTaken = 2*STEPS;
            nextButton.inactive = true;
            prevButton.inactive = true;
            if (currentSelectedPage >= pages[1].subObjects.Count) {
                currentSelectedPage = 0;
            }
        }
        if (message == "PREV" && stepsTaken == STEPS) {
            PlaySound(SoundID.MENU_Next_Slugcat, 0, 1.4f, 0.4f);
            currentSelectedPage--;
            stepsTaken = 0;
            nextButton.inactive = true;
            prevButton.inactive = true;
            if (currentSelectedPage < 0) {
                currentSelectedPage = pages[1].subObjects.Count-1;
            }
        }
    }
}