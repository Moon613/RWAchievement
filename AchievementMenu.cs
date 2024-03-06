using System;
using System.Collections.Generic;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
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
    public MenuTabWrapper tabWrapper;
    public OpScrollBox achievementScroll;
    public AchievementMenu(ProcessManager processManager) : base(processManager, Plugin.AchievementMenu) {
        mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
        currentPage = 0;
        stepsTaken = STEPS;
        movementPerStep = screenWidth/STEPS;

        #region Page 1
        pages.Add(new Page(this, null, "main", 0){pos=new Vector2(1366, 768) - manager.rainWorld.options.ScreenSize});
        tabWrapper = new MenuTabWrapper(this, pages[0]);
        pages[0].subObjects.Add(tabWrapper);

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
       ListViewButton listView = new ListViewButton(this, pages[0], "LIST", new Vector2(0.05f*screenWidth - 25, 0.95f*screenHeight - 25) - adjustForPageOffsetDueToResolution, new Vector2(48f, 48f));
        pages[0].subObjects.Add(listView);
        #endregion

        #region Page 2
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
        #endregion

        #region List View
        if (achievements.TryGetValue(manager.rainWorld, out List<Achievement> achievementList1)) {
            int scrollMenuRolls = Mathf.CeilToInt(achievementList1.Count/3);
            const int buttonHeight = 50;

            achievementScroll = new OpScrollBox(new Vector2(0.5f*screenCenter.x, (34f/768f)*screenHeight), new Vector2(800, (700f/768f)*screenHeight), buttonHeight*scrollMenuRolls, false, false, true);
            new UIelementWrapper(tabWrapper, achievementScroll);

            // for (int i = 0; i < achievementList1.Count; i++) {
                AchievementSimpleButton simpleButton = new AchievementSimpleButton(new Vector2(-25, buttonHeight*scrollMenuRolls+25), new Vector2(40, buttonHeight), "AAAAAAAAAA", "BBBBBBBBBB");
                var onClick = typeof(AchievementSimpleButton).GetEvent("OnClick");
                onClick.AddEventHandler(simpleButton, Delegate.CreateDelegate(onClick.EventHandlerType, this, typeof(AchievementMenu).GetMethod("Signal")));
                achievementScroll.AddItemToWrapped(simpleButton);
            // }
        }
        #endregion

        // pages[1].subObjects.Add(new AchievementPage(this, pages[1], "ach1", 2, new Vector2(screenWidth/2f + 2*screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, "", "aidesktopimg", "ACHIEVEMENT NAME", "2/27/2024", "ACHIEVEMENT\nDESCRIPTION"));
        
        // pages[1].subObjects.Add(new AchievementPage(this, pages[1], "ach2", 3, new Vector2((screenWidth/2f) + 3*screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, "", "full_figure_red", "ACHIEVEMENT NAME 2", "2/27/2024", "ACHIEVEMENT DESCRIPTION\n2"));
    }
    public override void Update()
    {
        base.Update();
        if (Input.GetKey(KeyCode.Y)) {
            currentPage = 2;
        }
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
    public void Signal(UIelement sennder) {
        Debug.Log((sennder as AchievementSimpleButton)?._text);
        Debug.Log((sennder as AchievementSimpleButton)?.ID);
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
public class AchievementSimpleButton : OpSimpleButton
{
    public string ID;
    public AchievementSimpleButton(Vector2 pos, Vector2 size, string displayText, string ID) : base(pos, size, displayText)
    {
        this.ID = ID;
    }
}
public class ListViewButton : ButtonTemplate
{
    public RoundedRect roundedRect;
    public string signalText;
    public FSprite symbolSprite;
    public bool maintainOutlineColorWhenGreyedOut;
    public ListViewButton(Menu.Menu menu, MenuObject owner, string singalText, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
    {
        signalText = singalText;
        roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, true);
        subObjects.Add(roundedRect);
        symbolSprite = new FSprite("Menu_Symbol_Show_List", true);
        Container.AddChild(symbolSprite);
    }
    public override void Update()
    {
        base.Update();
        buttonBehav.Update();
        roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
        roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? 0f : 1f);
    }
    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        float num = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * 3.1415927f * 2f);
        num *= buttonBehav.sizeBump;
        symbolSprite.scale = 2.75f;
        symbolSprite.color = (buttonBehav.greyedOut ? Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey) : Color.Lerp(base.MyColor(timeStacker), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey), num));
        symbolSprite.x = DrawX(timeStacker) + base.DrawSize(timeStacker).x / 2f;
        symbolSprite.y = DrawY(timeStacker) + base.DrawSize(timeStacker).y / 2f;
        Color color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
        for (int i = 0; i < 9; i++)
        {
            roundedRect.sprites[i].color = color;
        }
    }
    public override Color MyColor(float timeStacker)
    {
        if (!buttonBehav.greyedOut)
        {
            float num = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
            num = Mathf.Max(num, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
            HSLColor from = HSLColor.Lerp(Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey), Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey), num);
            return HSLColor.Lerp(from, Menu.Menu.MenuColor(Menu.Menu.MenuColors.Black), black).rgb;
        }
        if (maintainOutlineColorWhenGreyedOut)
        {
            return Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
        }
        return HSLColor.Lerp(Menu.Menu.MenuColor(Menu.Menu.MenuColors.VeryDarkGrey), Menu.Menu.MenuColor(Menu.Menu.MenuColors.Black), black).rgb;
    }
    public override void RemoveSprites()
    {
        symbolSprite.RemoveFromContainer();
        base.RemoveSprites();
    }
    public override void Clicked()
    {
        Singal(this, signalText);
    }
}