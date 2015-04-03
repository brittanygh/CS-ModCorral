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

         this.width = ModCorralUI.GeneralWidth;
         this.height = ModCorralUI.GeneralHeight - this.relativePosition.y - 5;
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         //this.builtinKeyNavigation = true;
         this.autoLayout = false;
         this.clipChildren = false; //temp
         int inset = 5;

         Log.Message("uiscrollbuttonpanel widht=" + this.width.ToString());

         ScrollPanel.relativePosition = new Vector3(inset, inset, 0);
         ScrollPanel.backgroundSprite = "GenericPanel";
         ScrollPanel.autoSize = false;
         ScrollPanel.width = (ModCorralUI.GeneralWidth - 25 - 4 * inset);
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
         ScrollBar.relativePosition = new Vector3(ScrollPanel.relativePosition.x + ScrollPanel.width + inset, ScrollPanel.relativePosition.y, 0);
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

      public UIButton AddAButton(string name, string text, string hovertext, Action<string> modCallback, string spritename, Texture2D texture)
      {
         Log.Message("adding button " + name + text + "    scrollpanelwidth=" + ScrollPanel.width.ToString());

         UIButton retval = ScrollPanel.AddUIComponent<UIButton>();

         string uniquename = string.Format("{0}_{1}", name, text);
         retval.name = uniquename;
         retval.cachedName = uniquename;

         if (spritename == null)
         {
            retval.text = text;
            retval.textPadding = new RectOffset(5, 5, 2, 2);
            retval.textHorizontalAlignment = UIHorizontalAlignment.Left;
            retval.normalBgSprite = "ButtonMenu";
            retval.hoveredBgSprite = "ButtonMenuHovered";
            retval.pressedBgSprite = "ButtonMenuPressed";
         }
         else
         {
            if (texture != null)
            {
               retval.atlas = CreateAtlas(spritename, texture);
            }

            retval.normalFgSprite = spritename;
            retval.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            retval.hoveredColor = new Color32(0, 255, 0, 255);
         }

         retval.tooltip = hovertext;
         retval.tooltipAnchor = UITooltipAnchor.Floating;
         
         retval.autoSize = false;
         
         if (spritename == null)
         {
            retval.height = 33;
            retval.width = (ModCorralUI.GeneralWidth - 25 - 4 * 5) - 4;
         }
         else
         {
            retval.height = 50;
            retval.width = 50;
         }

         
         retval.enabled = true;
         retval.isInteractive = true;
         retval.isVisible = true;

         retval.eventClick += (component, param) => 
         {            
            try
            {
               if (modCallback != null)
               {
                  modCallback(component.name);
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
            this.RemoveUIComponent(foundButton);
            UnityEngine.Object.Destroy(foundButton);
         }
      }

      UITextureAtlas CreateAtlas(string spriteName, Texture2D spriteTexture)
      {
         Texture2D atlasTex = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);

         Texture2D[] textures = new Texture2D[1];
         Rect[] rects = new Rect[1];

         textures[0] = spriteTexture;

         rects = atlasTex.PackTextures(textures, 2, 1024);

         UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();

         Material material = (Material)Material.Instantiate(UIView.GetAView().defaultAtlas.material);
         material.mainTexture = atlasTex;

         atlas.material = material;
         atlas.name = spriteName + "_atlas";

         UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo()
         {
            name = spriteName,
            texture = atlasTex,
            region = rects[0]
         };

         atlas.AddSprite(spriteInfo);

         return atlas;
      }
   }
}


