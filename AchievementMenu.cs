using System;
using System.Collections.Generic;
using System.Linq;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;
using static RWAchievements.Plugin;

namespace RWAchievements;
#pragma warning disable IDE0090
#pragma warning disable IDE0220
#pragma warning disable IDE0290
#pragma warning disable IDE0305
internal class AchievementMenu : Menu.Menu
{
    public int currentSelectedPage = 2;
    float ScreenWidth => manager.rainWorld.options.ScreenSize.x;
    float ScreenHeight => manager.rainWorld.options.ScreenSize.y;
    Vector2 ScreenCenter => manager.rainWorld.options.ScreenSize/2f;
    float GreatestXPos => (ScreenWidth/2f) + ((pages[1].subObjects.FindAll(obj => obj is AchievementPage).Count-2)*ScreenWidth) - pages[1].pos.x;
    float SmallestXPos => -ScreenWidth - 0.5f*ScreenWidth - pages[1].pos.x;
    const int PAGE_STEPS = 30;
    readonly float pageMovementPerStep;
    int pageStepsTaken;
    const int DROPDOWN_MENU_STEPS = 4;
    readonly float dropdownMovementPerStep;
    int dropdownStepsTaken;
    bool viewingList = false;
    const int MINIMUM_SCREEN_WIDTH = 1024;
    public int backCooldown = 0;
    internal int drawAllScenes = 0;
    public BigArrowButton prevButton;
    public BigArrowButton nextButton;
    public MenuTabWrapper tabWrapper;
    public OpScrollBox achievementScroll;
    public new SimpleButton backObject;
    public AchievementMenu(ProcessManager processManager) : base(processManager, Plugin.AchievementMenuID) {
        Debug.Log($"Aspect ratio is {Custom.rainWorld.options.ScreenSize}. If it is not 16:9 there may be problems with fading");
        currentPage = 0;

        pageStepsTaken = PAGE_STEPS;
        pageMovementPerStep = ScreenWidth/PAGE_STEPS;

        dropdownStepsTaken = 0;
        dropdownMovementPerStep = ScreenHeight/DROPDOWN_MENU_STEPS;

        // This creates the main container that holds all of the control UI buttons. Refered to as a "Page" by the vanilla game code.
        #region Main Page
        pages.Add(new Page(this, null, "main", 0){pos=new Vector2(1366, 768) - manager.rainWorld.options.ScreenSize});
        // This is a wrapper for Remix Op elements provided by Downpour, so that they can be used by other parts of the game.
        // This is used for the scrolling list view later.
        tabWrapper = new MenuTabWrapper(this, pages[0]);
        pages[0].subObjects.Add(tabWrapper);

        
        const int BigArrowButtonWidth = 25;
        Vector2 adjustForPageOffsetDueToResolution = 0.5f*pages[0].pos;

        // The next page button
        nextButton = new BigArrowButton(this, pages[0], "NEXT", new Vector2((ScreenWidth/2f) + 200f - BigArrowButtonWidth, 50f) - adjustForPageOffsetDueToResolution, 1);
        pages[0].subObjects.Add(nextButton);

        // The prev page button
        prevButton = new BigArrowButton(this, pages[0], "PREV", new Vector2((ScreenWidth/2f) - 200f - BigArrowButtonWidth, 50f) - adjustForPageOffsetDueToResolution, -1);
        pages[0].subObjects.Add(prevButton);

        // The back button lol
        const int BackButtonWidth = 160;
        SimpleButton backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(ScreenCenter.x-(BackButtonWidth/2), 50f) - adjustForPageOffsetDueToResolution, new Vector2(BackButtonWidth, 50f));
        pages[0].subObjects.Add(backButton);
        backObject = backButton;
        backButton.nextSelectable[0] = prevButton;
        backButton.nextSelectable[2] = nextButton;

        // List view button
        ListViewButton listView = new ListViewButton(this, pages[0], "LIST", new Vector2(0.05f*ScreenWidth - 25, 0.95f*ScreenHeight - 25) - adjustForPageOffsetDueToResolution, new Vector2(48f, 48f));
        pages[0].subObjects.Add(listView);
        #endregion

        #region Achievement Pages
        pages.Add(new Page(this, null, "achipages", 1){pos=new Vector2(1366, 768) - manager.rainWorld.options.ScreenSize});
        adjustForPageOffsetDueToResolution = 0.5f*pages[1].pos;
        // Removes the mouseCursor from the subObjects.
        pages[1].subObjects.Clear();
        
        if (achievementCWT.TryGetValue(manager.rainWorld, out List<Achievement> achievementList)) {
            for (int i = 0; i < achievementList.Count; i++) {
                Achievement achievement = achievementList[i];
                Debug.Log($"Created an achievement page for: {achievement}");
                pages[1].subObjects.Add(new AchievementPage(this, pages[1], achievement.achievementName, i, new Vector2(ScreenWidth/2f + (i-2) * ScreenWidth, ScreenHeight/2f) - adjustForPageOffsetDueToResolution, achievement));
            }
        }
        #endregion

        #region List View (Part of page 1)
        if (achievementCWT.TryGetValue(manager.rainWorld, out List<Achievement> achievementList1)) {
            int scrollMenuRows = Mathf.CeilToInt(achievementList1.Count/3f);
            const int buttonHeight = 50;

            achievementScroll = new OpScrollBox(new Vector2(ScreenWidth-MINIMUM_SCREEN_WIDTH, (50f/768f)*ScreenHeight+ScreenHeight), new Vector2(800, (700f/768f)*ScreenHeight), (buttonHeight+20)*scrollMenuRows, false, false, true);

            new UIelementWrapper(tabWrapper, achievementScroll);

            tabWrapper.myContainer.MoveToFront();

            // This shade sprite is the darkness that appears behind the list view to blur the achivement behind it out.
            FSprite shade = new FSprite("Futile_White")
            {
                scaleX = 86,
                scaleY = 49,
                color = new Color(0f, 0f, 0f, 0.95f)
            };
            shade.SetPosition(new Vector2(342,340));
            achievementScroll.myContainer.AddChild(shade);
            shade.MoveToBack();

            foreach (FSprite sprite in listView.roundedRect.sprites) {
                sprite.MoveToFront();
            }
            listView.symbolSprite.MoveToFront();

            // Adds buttons to the scroll view list, one for each achivement. They cause a jump to the an achievement.
            int rowNum = 1;
            for (int i = 0; i < achievementList1.Count; i++) {
                if (i%3==0 && i!=0) {
                    rowNum++;
                }
                string name = Translate(achievementList1[i].achievementName);
                // Trim names that are too long
                if (name.Length > 29) {
                    name = name.Substring(0, 29).Trim() + "...";
                }
                AchievementOPSimpleButton simpleButton = new AchievementOPSimpleButton(new Vector2(240*(i%3)+5, (buttonHeight+20)*scrollMenuRows-70*rowNum), new Vector2(600f/3f, buttonHeight), name, achievementList1[i].internalID);

                // Unfortunatly, the Event must be subscribed to using reflection via a delegate. But all it does is
                // makes the newly created button call the main menu's Signal method when it is clicked via the OnClick event.
                var onClick = typeof(AchievementOPSimpleButton).GetEvent("OnClick");
                onClick.AddEventHandler(simpleButton, Delegate.CreateDelegate(onClick.EventHandlerType, this, typeof(AchievementMenu).GetMethod("Signal")));

                achievementScroll.AddItemToWrapped(simpleButton);
            }
        }
        #endregion
        JumpToAchievement(0);
        Debug.Log($"Achievement Mod menu startup, current selected page: {currentSelectedPage}, out of {achievementList.Count}");

        // pages[1].subObjects.Add(new AchievementPage(this, pages[1], "ach1", 2, new Vector2(screenWidth/2f + 2*screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, "", "aidesktopimg", "ACHIEVEMENT NAME", "2/27/2024", "ACHIEVEMENT\nDESCRIPTION"));
        
        // pages[1].subObjects.Add(new AchievementPage(this, pages[1], "ach2", 3, new Vector2((screenWidth/2f) + 3*screenWidth, screenHeight/2f) - adjustForPageOffsetDueToResolution, "", "full_figure_red", "ACHIEVEMENT NAME 2", "2/27/2024", "ACHIEVEMENT DESCRIPTION\n2"));
    }
    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        pages[0].mouseCursor.BumToFront();
    }
    public override void Update()
    {
        if (drawAllScenes > 0) {
            drawAllScenes--;
        }
        base.Update();
        if (Input.GetKey(KeyCode.Escape) && backCooldown == 0)
        {
            if (!viewingList) {
                Singal(backObject, "BACK");
            }
            else {
                Singal(pages[0], "LIST");
            }
            backCooldown = 60;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            Singal(pages[0], "PREV");
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            Singal(pages[0], "NEXT");
        }
        if (Input.GetKey(KeyCode.L)) {
            Singal(pages[0], "LIST");
            backCooldown = 60;
        }
        // This moves all pages to the right
        if (pageStepsTaken < PAGE_STEPS) {
            foreach (AchievementPage page in pages[1].subObjects.FindAll(obj => obj is AchievementPage)) {
                if (Mathf.Round(page.pos.x) >= GreatestXPos) {
                    page.pos.x = SmallestXPos;
                    page.prevPos = page.pos;
                    page.lastPos = page.pos;
                }
                page.pos.x = Mathf.SmoothStep(page.prevPos.x, page.prevPos.x+pageMovementPerStep*PAGE_STEPS, pageStepsTaken/(PAGE_STEPS-1f));
            }
            pageStepsTaken++;
        }
        // This moves all pages to the left
        if (pageStepsTaken > PAGE_STEPS) {
            foreach (AchievementPage page in pages[1].subObjects.FindAll(obj => obj is AchievementPage)) {
                if (Mathf.Round(page.pos.x) <= SmallestXPos) {
                    page.pos.x = GreatestXPos;
                    page.prevPos = page.pos;
                    page.lastPos = page.pos;
                }
                page.pos.x = Mathf.SmoothStep(page.prevPos.x, page.prevPos.x-pageMovementPerStep*PAGE_STEPS, (2f*PAGE_STEPS-pageStepsTaken)/(PAGE_STEPS-1f));
            }
            pageStepsTaken--;
        }
        // Debug.Log($"Achievement Mod {(pages[1].subObjects[0] as AchievementPage)?.pos.x}, {(pages[1].subObjects[pages[1].subObjects.Count-1] as AchievementPage)?.pos.x}, {smallestXPos}");
        if (pageStepsTaken == PAGE_STEPS && !viewingList) {
            nextButton.buttonBehav.greyedOut = false;
            prevButton.buttonBehav.greyedOut = false;
            // This *should* make it so that there are no floating point errors in the x position.
            foreach (AchievementPage page in pages[1].subObjects.FindAll(obj => obj is AchievementPage)) {
                page.pos.x = Mathf.RoundToInt(page.pos.x);
                page.prevPos = page.pos;
                // Debug.Log($"Achievement Mod alignment test: {page.pos.x%20}");
            }
        }
        // These are for the dropdown menu, or List view, for sending it up and down when triggered.
        if (dropdownStepsTaken > 0) {
            achievementScroll.PosY -= dropdownMovementPerStep;
            dropdownStepsTaken--;
        }
        if (dropdownStepsTaken < 0) {
            achievementScroll.PosY += dropdownMovementPerStep;
            dropdownStepsTaken++;
        }
        if (backCooldown > 0) {
            backCooldown--;
        }
    }
    // This is used by the buttons in the scrolling list view
    public void Signal(UIelement sennder) {
        Debug.Log((sennder as AchievementOPSimpleButton)?._text);
        Debug.Log((sennder as AchievementOPSimpleButton)?.ID);
        List<UIelement> achievementScrollAsList = achievementScroll.items.ToList();
        for (int i = 0; i < achievementScrollAsList.Count; i++) {
            if ((achievementScrollAsList[i] as AchievementOPSimpleButton)?.ID == (sennder as AchievementOPSimpleButton)?.ID) {
                JumpToAchievement(i);
                if (currentSelectedPage >= pages[1].subObjects.FindAll(obj => obj is AchievementPage).Count) {
                    currentSelectedPage -= achievementScrollAsList.Count;
                }
                if (currentSelectedPage < 0) {
                    currentSelectedPage = pages[1].subObjects.FindAll(obj => obj is AchievementPage).Count-currentSelectedPage;
                }
                Debug.Log($"Achievement Mod Index of page: {i}, currently selected page: {currentSelectedPage}");
                break;
            }
        }
        Singal(sennder.Owner, "LIST");
    }
    public void JumpToAchievement(int pageNum)
    {
        foreach (AchievementPage page in pages[1].subObjects.FindAll(obj => obj is AchievementPage)) {
            page.scene.hidden = false;
            drawAllScenes = 40;
            page.pos.x -= (pageNum-currentSelectedPage)*PAGE_STEPS*pageMovementPerStep;
            if (page.pos.x > GreatestXPos) {
                page.pos.x -= GreatestXPos + -SmallestXPos;
            }
            if (page.pos.x < SmallestXPos) {
                page.pos.x += GreatestXPos + -SmallestXPos;
            }
            page.lastPos = page.pos;
            page.prevPos = page.pos;
        }
        currentSelectedPage = pageNum;
    }
    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if (message == "BACK") {
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            PlaySound(SoundID.MENU_Switch_Page_Out);
            manager.menuMic.PlayLoop(SoundID.MENU_Main_Menu_LOOP, 0, 1, 1, true);
        }
        if (message == "NEXT" && pageStepsTaken == PAGE_STEPS) {
            PlaySound(SoundID.MENU_Next_Slugcat, 0.4f, 0.8f, 0.9f);
            currentSelectedPage++;
            pageStepsTaken = 2*PAGE_STEPS;
            nextButton.buttonBehav.greyedOut = true;
            prevButton.buttonBehav.greyedOut = true;
            if (currentSelectedPage >= pages[1].subObjects.FindAll(obj => obj is AchievementPage).Count) {
                currentSelectedPage = 0;
            }
            Debug.Log($"Achievement Mod next page, current selected page: {currentSelectedPage}");
        }
        if (message == "PREV" && pageStepsTaken == PAGE_STEPS) {
            PlaySound(SoundID.MENU_Next_Slugcat, -0.4f, 0.8f, 0.9f);
            currentSelectedPage--;
            pageStepsTaken = 0;
            nextButton.buttonBehav.greyedOut = true;
            prevButton.buttonBehav.greyedOut = true;
            if (currentSelectedPage < 0) {
                currentSelectedPage = pages[1].subObjects.FindAll(obj => obj is AchievementPage).Count-1;
            }
            Debug.Log($"Achievement Mod prev page, current selected page: {currentSelectedPage}");
        }
        if (message == "LIST" && dropdownStepsTaken == 0) {
            if (!viewingList) {
                PlaySound(SoundID.MENU_Checkbox_Check, 0, 0.4f, 1);
                dropdownStepsTaken = DROPDOWN_MENU_STEPS;
                nextButton.buttonBehav.greyedOut = true;
                prevButton.buttonBehav.greyedOut = true;
                backObject.buttonBehav.greyedOut = true;
                viewingList = true;
            }
            else {
                PlaySound(SoundID.MENU_Checkbox_Uncheck, 0, 0.4f, 1);
                dropdownStepsTaken = -DROPDOWN_MENU_STEPS;
                nextButton.buttonBehav.greyedOut = false;
                prevButton.buttonBehav.greyedOut = false;
                backObject.buttonBehav.greyedOut = false;
                viewingList = false;
            }
        }
    }
}
internal class AchievementOPSimpleButton : OpSimpleButton
{
    public string ID;
    public AchievementOPSimpleButton(Vector2 pos, Vector2 size, string displayText, string ID) : base(pos, size, displayText)
    {
        this.ID = ID;
    }
}
internal class ListViewButton : ButtonTemplate
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
        roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * Mathf.PI)) * (buttonBehav.clicked ? 0f : 1f);
    }
    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        symbolSprite.scale = 2.75f;
        symbolSprite.color = (buttonBehav.greyedOut ? Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey) : base.MyColor(timeStacker));
        symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
        symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
        Color color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
        for (int i = 0; i < 9; i++)
        {
            roundedRect.sprites[i].color = color;
        }
    }
    public override Color MyColor(float timeStacker)
    {
        if (!buttonBehav.greyedOut) {
            float num = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
            num = Mathf.Max(num, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
            HSLColor from = HSLColor.Lerp(Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey), Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey), num);
            return HSLColor.Lerp(from, Menu.Menu.MenuColor(Menu.Menu.MenuColors.Black), black).rgb;
        }
        if (maintainOutlineColorWhenGreyedOut) {
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