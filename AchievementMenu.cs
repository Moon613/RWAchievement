
using Menu;
using UnityEngine;

namespace AchievementMenu;
public class AchievementMenu : Menu.Menu
{
    public SimpleButton backButton;
    private readonly float leftAnchor;
    public AchievementMenu(ProcessManager processManager) : base(processManager, Plugin.AchievementMenu) {
        mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
        leftAnchor = (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
        currentPage = 0;

        pages.Add(new Page(this, null, "main", 0));
        pages[0].subObjects.Add(new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.MainMenu_Downpour));
        pages[0].subObjects[0].myContainer.AddChild(new FSprite("Futile_White"){scale=20});

        pages.Add(new Page(this, null, "main2", 1));
        pages[1].pos.x = 1000;

        // The next page button
        pages[0].subObjects.Add(new BigArrowButton(this, pages[0], "NEXT", new Vector2(leftAnchor + 800f, 75f), 1));
        
        // The prev page button
        pages[0].subObjects.Add(new BigArrowButton(this, pages[0], "PREV", new Vector2(leftAnchor + 500f, 75f), -1));
        
        backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(leftAnchor + 15f, 50f), new Vector2(220f, 30f));
        pages[0].subObjects.Add(backButton);
        backObject = backButton;
        backButton.nextSelectable[0] = backButton;
        backButton.nextSelectable[2] = backButton;
    }
    public override void Update()
    {
        base.Update();
        if (Input.GetKey(KeyCode.Escape))
        {
            Singal(backObject, "BACK");
        }
    }
    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if (message == "BACK") {
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            PlaySound(SoundID.MENU_Switch_Page_Out);
        }
        if (message == "NEXT") {
            PlaySound(SoundID.MENU_Next_Slugcat, 0, 1.4f, 0.4f);
            currentPage++;
            pages[currentPage-1].pos.x -= 1000;
            if (currentPage >= pages.Count) {
                currentPage = 0;
            }
            pages[currentPage].pos.x = 0;
        }
        if (message == "PREV") {
            PlaySound(SoundID.MENU_Next_Slugcat, 0, 1.4f, 0.4f);
            currentPage--;
            if (currentPage < 0) {
                currentPage = pages.Count-1;
            }
        }
    }
}