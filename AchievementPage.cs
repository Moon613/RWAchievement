using System.Collections.Generic;
using RWCustom;
using MenuShader = Menu.MenuDepthIllustration.MenuShader;
using Menu;
using UnityEngine;

namespace RWAchievements;
#pragma warning disable IDE0090
#pragma warning disable IDE0028
#pragma warning disable IDE0044

public class AchievementPage : Page
{
    MenuLabel title;
    MenuLabel date;
    MenuLabel? origin;
    MenuLabel description;
    internal InteractiveMenuScene scene;
    List<MenuIllustration> menuIllustrations = new List<MenuIllustration>();
    Vector2 ScreenCenter => menu.manager.rainWorld.options.ScreenSize/2f;
    public Vector2 prevPos;
    private float IconSize => 256f;
    public AchievementPage(Menu.Menu menu, Page owner, string name, int index, Vector2 pos, Achievement achievement) : base(menu, owner, name, index) {
        this.owner = owner;
        // Clear it of the mouse position object, I won't be needing it
        mouseCursor.RemoveSprites();
        mouseCursor = null;
        subObjects.Clear();
        scene = new InteractiveMenuScene(menu, owner, MenuScene.SceneID.Empty);
        subObjects.Add(scene);
        // Set the default position
        this.pos = pos;
        
        // Add the Achievement name
        title = new MenuLabel(menu, this, achievement.unlocked? achievement.achievementName : "Hidden Achievement", new Vector2(0, 0.85f*ScreenCenter.y), Vector2.zero, true);
        title.label.scale = 2.5f;
        subObjects.Add(title);
        
        // Add the date it was achieved
        date = new MenuLabel(menu, this, "Date Achieved:\n" + (achievement.unlocked? achievement.dateAchieved : "?"), new Vector2(0, 0.695f*ScreenCenter.y), Vector2.zero, true);
        date.label.scale = 0.65f;
        subObjects.Add(date);
        
        // Add text that identifies which mod it came from, if specified
        if (achievement.originMod != null) {
            origin = new MenuLabel(menu, this, "From: " + achievement.originMod, new Vector2(0, 0.61f*ScreenCenter.y), Vector2.zero, true);
            origin.label.scale = 0.575f;
            subObjects.Add(origin);
        }
        
        // Add the Achievement image
        image = new MenuIllustration(menu, this, achievement.imageFolder, achievement.unlocked? achievement.imageName : "multiplayerportrait02", new Vector2(0, 0.16f*screenCenter.y), true, true);
        if (achievement.originMod == "Steam") {
            var newElementSize = image.sprite.element.sourceSize;
            image.sprite.scaleX = IconSize / newElementSize.x;
            image.sprite.scaleY = IconSize / newElementSize.y;
        }
        subObjects.Add(image);
        
        // Add the description
        description = new MenuLabel(menu, this, achievement.unlocked? achievement.description : "???", new Vector2(0, -0.5f*ScreenCenter.y), Vector2.zero, true);
        description.label.scale = 1.01f;
        subObjects.Add(description);
    }
    public override void Update()
    {
        base.Update();
        
        // This stops offscreen scenes from being drawn at all, otherwise I think it would mess with the setting of shader globals
        if (pos.x <= Custom.GetScreenOffsets()[1] && pos.x >= -0.5f*Custom.GetScreenOffsets()[1]) {
            scene.hidden = false;
        }
        // But don't be hidden if this is true, so that moving around using the list view works
        else if (menu is AchievementMenu achievementMenu && achievementMenu.drawAllScenes <= 0) {
            scene.hidden = true;
        }

        float alpha = Mathf.Lerp(1, 0, (1f/256f)*Mathf.Abs(menu.manager.rainWorld.options.ScreenSize.x*0.5f-pos.x));
        Color color;
        
        // Sets the alpha of the images, and adjusts the position of the depth illustrations
        foreach (MenuIllustration illustration in menuIllustrations) {
            illustration.alpha = alpha;
            if (illustration is MenuDepthIllustration depthIllustration) {
                depthIllustration.pos.x = pos.x - ScreenCenter.x - 50;
                depthIllustration.pos.y = -50;
            }
        }

        color = title.label.color;
        color.a = alpha;
        title.label.color = color;

        color = date.label.color;
        color.a = alpha;
        date.label.color = color;

        if (origin != null) {
            color = origin.label.color;
            color.a = alpha;
            origin.label.color = color;
        }

        color = description.label.color;
        color.a = alpha;
        description.label.color = color;
    }
}