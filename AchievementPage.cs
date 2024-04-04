using Menu;
using UnityEngine;

namespace RWAchievements;

public class AchievementPage : Page
{
    MenuLabel title;
    MenuLabel date;
    MenuLabel? origin;
    MenuIllustration image;
    MenuLabel description;
    Vector2 screenCenter => menu.manager.rainWorld.options.ScreenSize/2f;
    new Page owner;
    public Vector2 prevPos;
    private float IconSize => 256f;
    public AchievementPage(Menu.Menu menu, Page owner, string name, int index, Vector2 pos, Achievement achievement) : base(menu, owner, name, index) {
        this.owner = owner;
        // Clear it of the mouse position object, I won't be needing it
        subObjects.Clear();
        // Set the default position
        this.pos = pos;
        
        // Add the Achievement name
        title = new MenuLabel(menu, this, achievement.unlocked? achievement.achievementName : "Hidden Achievement", new Vector2(0, 0.85f*screenCenter.y), Vector2.zero, true);
        title.label.scale = 2.5f;
        subObjects.Add(title);
        
        // Add the date it was achieved
        date = new MenuLabel(menu, this, "Date Achieved:\n" + (achievement.unlocked? achievement.dateAchieved : "?"), new Vector2(0, 0.695f*screenCenter.y), Vector2.zero, true);
        date.label.scale = 0.65f;
        subObjects.Add(date);
        
        // Add text that identifies which mod it came from, if specified
        if (achievement.originMod != null) {
            origin = new MenuLabel(menu, this, "From: " + achievement.originMod, new Vector2(0, 0.61f*screenCenter.y), Vector2.zero, true);
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
        description = new MenuLabel(menu, this, achievement.unlocked? achievement.description : "???", new Vector2(0, -0.5f*screenCenter.y), Vector2.zero, true);
        description.label.scale = 1.01f;
        subObjects.Add(description);
    }
    public override void Update()
    {
        base.Update();
        float alpha = Mathf.Lerp(1, 0, (1f/256f)*Mathf.Abs(menu.manager.rainWorld.options.ScreenSize.x*0.5f-pos.x));

        Color color = image.sprite.color;
        color.a = alpha;
        image.sprite.color = color;

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