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

         this.width = 383;
         this.height = 903 - this.relativePosition.y - 5;
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         //this.builtinKeyNavigation = true;
         this.autoLayout = false;
         this.clipChildren = true; //temp
         int inset = 5;

         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "uiscrollbuttonpanel widht=" + this.width.ToString());

         ScrollPanel.relativePosition = new Vector3(inset, inset, 0);
         ScrollPanel.backgroundSprite = "GenericPanel";
         ScrollPanel.autoSize = false;
         ScrollPanel.width = (383 - 25 - 4 * inset);
         ScrollPanel.height = (this.height - ScrollPanel.relativePosition.y - inset);
         ScrollPanel.autoLayout = true;
         ScrollPanel.isInteractive = true;
         ScrollPanel.clipChildren = true;
         ScrollPanel.useGUILayout = true;
         ScrollPanel.canFocus = true;

         ScrollPanel.autoLayoutDirection = LayoutDirection.Vertical;
         ScrollPanel.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
         ScrollPanel.autoLayoutStart = LayoutStart.TopLeft;

         //ScrollPanel.builtinKeyNavigation = true;//conflicts with onkeypress?
         ScrollPanel.scrollWithArrowKeys = false;

         //ScrollPanel.freeScroll = true;
         ScrollPanel.scrollWheelDirection = UIOrientation.Vertical;

         ScrollBar.useGUILayout = true;

         ScrollBar.width = 25;//?
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

      public UIButton AddAButton(string name, string text, string hovertext)
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "adding button " + name + text + "    scrollpanelwidth=" + ScrollPanel.width.ToString());

         UIButton retval = ScrollPanel.AddUIComponent<UIButton>();

         string uniquename = string.Format("{0}_{1}", name, text);
         retval.name = uniquename;
         retval.cachedName = uniquename;
         retval.text = text;
         retval.autoSize = false;
         
         retval.height = 32;
         retval.width = (383 - 25 - 4 * 5) - 4;
         retval.textPadding = new RectOffset(5, 5, 2, 2);
         retval.textHorizontalAlignment = UIHorizontalAlignment.Left;
         retval.normalBgSprite = "ButtonMenu";
         retval.hoveredBgSprite = "ButtonMenuHovered";
         retval.pressedBgSprite = "ButtonMenuPressed";
         retval.enabled = true;
         retval.isInteractive = true;
         retval.isVisible = true;

         retval.eventClick += (component, param) => { ParentPanel.HideMe(); };
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
   }
}


