using static RWAchievements.Plugin;
using RWCustom;
using UnityEngine;
using System.IO;
using System.Linq;

namespace RWAchievements;
#pragma warning disable IDE0017
#pragma warning disable IDE0090
#pragma warning disable IDE0290

public abstract class Popup
{
    private readonly Achievement _achievement;
    /// <summary>
    /// To be used with the lastPos field for linear interpolation in the GrafUpdate method.
    /// </summary>
    public Vector2 pos;
    /// <summary>
    /// To be used with the pos field for linear interpolation in the GrafUpdate method.
    /// </summary>
    public Vector2 lastPos;
    public Popup(Achievement achievement) {
        _achievement = achievement;
    }
    /// <summary>
    /// This is called as much as possible, but is only called once a frame.
    /// This should be used for smooth graphics, so that the graphical updates are not tied to the framerate.
    /// </summary>
    public virtual void GrafUpdate(float timeStacker) {
    }
    /// <summary>
    /// This is called 40 times a second under normal circumstances (no lag). Normal update logic goes here.
    /// </summary>
    public virtual void Update() {
    }
    /// <summary>
    /// This returns the achievement associated with thise Popup
    /// </summary>
    public Achievement GetAchievement() {
        return _achievement;
    }
}

public class DefaultPopup : Popup
{
    public float IconSize => 64f;
    public int timeIdle = 0;
    public int stepsToTake = 30;
    public float movementPerStep;
    public FSprite background;
    public FSprite image;
    public FLabel text;
    internal DefaultPopup(Achievement achievement) : base(achievement) {
        movementPerStep = (float)POPUP_HEIGHT/stepsToTake;
        pos = new Vector2(Futile.screen.pixelWidth-POPUP_WIDTH+1, -POPUP_HEIGHT);
        lastPos = pos;

        background = new FSprite("Futile_White");
        background.SetAnchor(0,0);
        background.SetPosition(pos);
        background.width = POPUP_WIDTH;
        background.height = POPUP_HEIGHT;
        background.shader = Custom.rainWorld.Shaders["AchievementPopup"];
        achievementPopupContainer.AddChild(background);

        image = new FSprite("Futile_White");
        if (Futile.atlasManager.DoesContainElementWithName(GetAchievement().flatImageName)) {
            image.element = Futile.atlasManager.GetElementWithName(GetAchievement().flatImageName);
        }
        else if (Futile.atlasManager.GetAtlasWithName(GetAchievement().flatImageName) != null)
        {
            FAtlas atlasWithName = Futile.atlasManager.GetAtlasWithName(GetAchievement().flatImageName);
            image.element = atlasWithName.elements.First();
        }
        else {
            string folder = (GetAchievement().imageFolder == "")? "illustrations" : GetAchievement().imageFolder;
			Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			string str = AssetManager.ResolveFilePath(folder + Path.DirectorySeparatorChar.ToString() + GetAchievement().flatImageName + ".png");
			string str2 = "file:///";
			try
			{
				AssetManager.SafeWWWLoadTexture(ref texture, str2 + str, true, true);
			}
			catch (FileLoadException arg)
			{
				Debug.LogError(new string[]
				{
					string.Format(format: "Error loading file: {0}", arg)
				});
			}
            Futile.atlasManager.UnloadAtlas(GetAchievement().flatImageName);
            var atlas = Futile.atlasManager.LoadAtlasFromTexture(GetAchievement().flatImageName, texture, false);
            image.element = atlas.elements.First();
        }
        Vector2 originalSize = image.element.sourceSize;
        image.scaleX = IconSize/originalSize.x;
        image.scaleY = IconSize/originalSize.y;
        image.SetPosition(pos);
        achievementPopupContainer.AddChild(image);
        image.MoveToFront();

        text = new FLabel(Custom.GetFont(), GetAchievement().achievementName);
        text.alignment = FLabelAlignment.Left;
        text.SetPosition(pos);
        achievementPopupContainer.AddChild(text);
        text.MoveToFront();
    }
    public override void GrafUpdate(float timeStacker) {
        background.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker));
        image.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) + new Vector2(45, 45));
        text.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) + new Vector2(85, 65));
    }
    public override void Update() {
        lastPos = pos;
        if (stepsToTake > 0) {
            pos.y += movementPerStep;
            stepsToTake--;
        }
        else if (stepsToTake < 0) {
            pos.y -= movementPerStep;
            stepsToTake++; 
        }
        else {
            timeIdle++;
        }
        if (timeIdle == 200) {
            stepsToTake = -80;
        }
        else if (timeIdle > 200) {
            popupList.Remove(this);
            background.RemoveFromContainer();
            image.RemoveFromContainer();
        }
    }
}