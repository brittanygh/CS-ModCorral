using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICities;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ModCorral
{
   // title bar with descriptive lable (non editable) and close button
   // holds draggable handle for moving window
   public class UIScrollButtonPanel : UIPanel
   {
      public ModCorralUI ParentPanel;

      public UIScrollablePanel ScrollPanel;
      public UIScrollbar ScrollBar;
      public UISlicedSprite ThumbSprite;
      public UISlicedSprite TrackSprite;

      // runs only once, do internal init here
      public override void Awake()
      {
         base.Awake();

         // Note - parent may not be set yet
         ScrollPanel = AddUIComponent<UIScrollablePanel>();
         ScrollBar = AddUIComponent<UIScrollbar>();
         TrackSprite = ScrollBar.AddUIComponent<UISlicedSprite>();
         ThumbSprite = TrackSprite.AddUIComponent<UISlicedSprite>();
      }

      // runs only once, do internal connections and setup here
      public override void Start()
      {
         base.Start();

         if (ParentPanel == null)
         {
            return;
         }

         int inset = 0;
         this.width = ModCorralUI.GeneralWidth;
         this.height = ModCorralUI.GeneralHeight - this.relativePosition.y - inset;
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         //this.builtinKeyNavigation = true;
         this.autoLayout = false;
         this.clipChildren = false; //temp

         ScrollPanel.relativePosition = new Vector3(inset, inset, 0);
         ScrollPanel.backgroundSprite = "SubcategoriesPanel";
         ScrollPanel.autoSize = false;
         ScrollPanel.width = (ModCorralUI.GeneralWidth - 20);
         ScrollPanel.height = (this.height - ScrollPanel.relativePosition.y - inset);
         ScrollPanel.autoLayout = true;
         ScrollPanel.isInteractive = true;
         ScrollPanel.clipChildren = true;
         ScrollPanel.useGUILayout = true;
         ScrollPanel.canFocus = true;

         ScrollPanel.autoLayoutDirection = LayoutDirection.Vertical;
         ScrollPanel.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
         ScrollPanel.autoLayoutStart = LayoutStart.TopLeft;

         ScrollPanel.builtinKeyNavigation = true;//conflicts with onkeypress?
         ScrollPanel.scrollWithArrowKeys = false;

         //ScrollPanel.freeScroll = true;
         ScrollPanel.scrollWheelDirection = UIOrientation.Vertical;

         ScrollBar.useGUILayout = true;

         ScrollBar.width = 20;//?25
         ScrollBar.height = ScrollPanel.height;
         ScrollBar.orientation = UIOrientation.Vertical;
         ScrollBar.isInteractive = true;
         ScrollBar.isVisible = true;
         ScrollBar.enabled = true;
         ScrollBar.relativePosition = new Vector3(ScrollPanel.relativePosition.x + ScrollPanel.width + 5, ScrollPanel.relativePosition.y, 0);
         ScrollBar.minValue = 0;
         ScrollBar.value = 0;
         ScrollBar.incrementAmount = 10;
         ScrollBar.maxValue = ScrollPanel.height;

         TrackSprite.relativePosition = Vector2.zero;
         TrackSprite.autoSize = true;
         TrackSprite.size = ScrollBar.size;
         TrackSprite.fillDirection = UIFillDirection.Horizontal;
         TrackSprite.spriteName = "ScrollbarTrack";
         ScrollBar.trackObject = TrackSprite;

         ThumbSprite.relativePosition = Vector2.zero;
         ThumbSprite.fillDirection = UIFillDirection.Horizontal;
         ThumbSprite.autoSize = true;
         ThumbSprite.width = TrackSprite.width;
         ThumbSprite.spriteName = "ScrollbarThumb";
         ScrollBar.thumbObject = ThumbSprite;

         ScrollPanel.verticalScrollbar = ScrollBar;
         ScrollPanel.scrollWheelAmount = 10;

         ScrollPanel.enabled = true;
      }

      public void ClearButtons()
      {
         UIComponent[] carray = new UIComponent[ScrollPanel.components.Count];
         ScrollPanel.components.CopyTo(carray, 0);

         foreach (UIComponent child in carray)
         {
            ScrollPanel.RemoveUIComponent(child);
            UnityEngine.Object.Destroy(child);
         }
      }

      public UIButton AddAButton(ModRegistrationInfo mri)
      {
         UIButton retval = ScrollPanel.AddUIComponent<UIButton>();

         string uniquename = string.Format("{0}_{1}", mri.ModName, mri.ButtonText);
         retval.name = uniquename;
         retval.cachedName = uniquename;

         if (mri.NormalFgSpritename == null) // no images, use a button with text in it
         {
            retval.text = mri.ButtonText;
            retval.textPadding = new RectOffset(2, 2, 2, 2);
            retval.textHorizontalAlignment = UIHorizontalAlignment.Left;
            retval.textScaleMode = UITextScaleMode.None;
            retval.textScale = 0.65f;
            retval.wordWrap = true;
            
            retval.normalBgSprite = "ButtonMenu";
            retval.hoveredBgSprite = "ButtonMenuHovered";
            retval.pressedBgSprite = "ButtonMenuPressed";
         }
         else
         {
            if (mri.NormalFgTexture != null) // this triggers use of all textures
            {
               string[] spritenames = { mri.NormalFgSpritename, mri.NormalBgSpritename, mri.HoveredFgSpritename, mri.HoveredBgSpritename, mri.PressedFgSpritename, mri.PressedBgSpritename };
               Texture2D[] textures = { mri.NormalFgTexture, mri.NormalBgTexture, mri.HoveredFgTexture, mri.HoveredBgTexture, mri.PressedFgTexture, mri.PressedBgTexture };

               retval.atlas = CreateAtlas(uniquename, spritenames, textures);
            }
            else
            {
               // built-in sprite names
               // - try to synthesize hover/focus/etc sprite names
               // - hover = name + "hovered"
               // - pressed = name + "pressed"
               string basename = mri.NormalFgSpritename;

               string hoversprite = basename + "Hovered";
               string pressedsprite = basename + "Pressed";

               if (string.IsNullOrEmpty(mri.HoveredFgSpritename))
                  mri.HoveredFgSpritename = hoversprite;
               if (string.IsNullOrEmpty(mri.PressedFgSpritename))
                  mri.PressedFgSpritename = pressedsprite;
            }

            if (string.IsNullOrEmpty(mri.HoveredBgSpritename))
               mri.HoveredBgSpritename = "OptionBaseHovered"; // so that everybody has some hover feedback, even if they don't specify it

            retval.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            retval.normalFgSprite = mri.NormalFgSpritename;
            retval.normalBgSprite = mri.NormalBgSpritename;
            retval.hoveredFgSprite = mri.HoveredFgSpritename;
            retval.hoveredBgSprite = mri.HoveredBgSpritename;
            retval.pressedFgSprite = mri.PressedFgSpritename;
            retval.pressedBgSprite = mri.PressedBgSpritename;

            retval.spritePadding = new RectOffset(2, 2, 2, 2);

         }

         retval.tooltip = mri.HoverText;
         retval.tooltipAnchor = UITooltipAnchor.Floating;
         retval.eventTooltipShow += (component, param) => { param.tooltip.relativePosition = new Vector3(param.tooltip.relativePosition.x + 25, param.tooltip.relativePosition.y, param.tooltip.relativePosition.z); };

         //retval.tooltipBox

         retval.autoSize = false;
         
         //if (mri.NormalFgSpritename == null)
         //{
         //   retval.height = 33;
         //   retval.width = (ModCorralUI.GeneralWidth - 25 - 4 * 5) - 4;
         //}
         //else
         {
            retval.height = 50;
            retval.width = 50;
         }

         retval.canFocus = false;
         retval.enabled = true;
         retval.isInteractive = true;
         retval.isVisible = true;

         retval.eventClick += (component, param) => 
         {            
            try
            {
               if (mri.ClickCallback != null)
               {
                  mri.ClickCallback(component.name);
               }
            }
            catch (Exception ex)
            {
               Log.Error(string.Format("Exception in callback to Mod: {0}. Exception: {1}", component.name, ex.Message));
            }

            ModCorral.mcButton.SimulateClick();
         };

         return retval;
      }

      public void RemoveAButton(string name)
      {
         UIButton foundButton = ScrollPanel.Find<UIButton>(name);

         if (foundButton != null)
         {
            ScrollPanel.RemoveUIComponent(foundButton);
            UnityEngine.Object.Destroy(foundButton);
         }
      }

      UITextureAtlas CreateAtlas(string atlasName, string[] spriteNames, Texture2D[] spriteTextures)
      {
         Texture2D atlasTex = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);

         Texture2D[] textures = spriteTextures;
         Rect[] rects = new Rect[1];

         rects = atlasTex.PackTextures(textures, 2, 1024);

         UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();

         Material material = (Material)Material.Instantiate(UIView.GetAView().defaultAtlas.material);
         material.mainTexture = atlasTex;

         atlas.material = material;
         atlas.name = atlasName;

         for (int i = 0; i < rects.Length; i++ )
         {
            if (spriteNames[i] != null && textures[i] != null)
            {
               UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo()
               {
                  name = spriteNames[i],
                  texture = textures[i],
                  region = rects[i]
               };

               atlas.AddSprite(spriteInfo);
            }
         }

         return atlas;
      }

         //UITextureAtlas atlas = new UITextureAtlas ();
         //atlas.material = new Material (ResourceUtils.GetUIAtlasShader ());

         //Texture2D texture = new Texture2D (0, 0);
         //Rect[] rects = texture.PackTextures (sprites, 0);

         //for (int i = 0; i < rects.Length; ++i) {
         //   Texture2D sprite = sprites [i];
         //   Rect rect = rects [i];

         //   UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo ();
         //   spriteInfo.name = sprite.name;
         //   spriteInfo.texture = sprite;
         //   spriteInfo.region = rect;
         //   spriteInfo.border = new RectOffset ();

         //   atlas.AddSprite (spriteInfo);
         //}

         //atlas.material.mainTexture = texture;
         //return atlas;
   }
}


