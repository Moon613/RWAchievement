using Menu;
using UnityEngine;

namespace AchievementMenu;

public class AchievementPage : Page
{
    float screenWidth => menu.manager.rainWorld.options.ScreenSize.x;
    float screenHeight => menu.manager.rainWorld.options.ScreenSize.y;
    Vector2 screenCenter => menu.manager.rainWorld.options.ScreenSize/2f;
    public AchievementPage(Menu.Menu menu, MenuObject owner, string name, int index, Vector2 pos, string imageFolder, string imageName, string achievementName, string dateAchieved, string achievementDescription, string originMod) : base(menu, owner, name, index) {
        // Clear it of the mouse position object, I won't be needing it
        subObjects.Clear();
        // Set the default position
        this.pos = pos;
        
        // Add the Achievement name
        MenuLabel title = new MenuLabel(menu, this, achievementName, new Vector2(0, 0.85f*screenCenter.y), Vector2.zero, true);
        title.label.scale = 2.5f;
        subObjects.Add(title);
        
        // Add the date it was achieved
        MenuLabel date = new MenuLabel(menu, this, "Date Achieved:\n" + dateAchieved, new Vector2(0, 0.695f*screenCenter.y), Vector2.zero, true);
        date.label.scale = 0.65f;
        subObjects.Add(date);
        
        // Add text that identifies which mod it came from, if specified
        if (originMod != "") {
            MenuLabel origin = new MenuLabel(menu, this, "From: " + originMod, new Vector2(0, 0.61f*screenCenter.y), Vector2.zero, true);
            origin.label.scale = 0.575f;
            subObjects.Add(origin);
        }
        
        // Add the Achievement image
        subObjects.Add(new MenuIllustration(menu, this, imageFolder, imageName, new Vector2(0, 0.16f*screenCenter.y), true, true));
        
        // Add the description
        MenuLabel description = new MenuLabel(menu, this, achievementDescription, new Vector2(0, -0.5f*screenCenter.y), Vector2.zero, true);
        description.label.scale = 1f;
        subObjects.Add(description);
    }
}