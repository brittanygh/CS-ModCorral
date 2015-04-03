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
   public class UITitleSubPanel : UIPanel
   {
      public ModCorralUI ParentPanel;

      public UILabel Title;
      public UIButton CloseButton;
      private UIDragHandle DragHandle;


      // runs only once, do internal init here
      public override void Awake()
      {
         base.Awake();

         // Note - parent may not be set yet

         Title = AddUIComponent<UILabel>();
         CloseButton = AddUIComponent<UIButton>();
         DragHandle = AddUIComponent<UIDragHandle>();
      }

      // runs only once, do internal connections and setup here
      public override void Start()
      {
         base.Start();

         if (ParentPanel == null)
         {
            return;
         }

         // setup now that we are in parent
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         this.relativePosition = Vector3.zero;

         int inset = 5;

         Title.relativePosition = Vector3.zero;//new Vector3(inset, inset);
         Title.autoSize = false;
         Title.width = this.width - CloseButton.width - inset;
         Title.height = this.height;
         Title.text = Locale.Get(LocaleID.CONTENTMANAGER_MODS);//"Mod Corral";
         //Title.textScale = 0.9f;

         Title.textAlignment = UIHorizontalAlignment.Left;
         Title.verticalAlignment = UIVerticalAlignment.Middle;
         Title.padding = new RectOffset(inset, inset, inset, inset);

         CloseButton.normalBgSprite = "buttonclose";
         CloseButton.hoveredBgSprite = "buttonclosehover";
         CloseButton.pressedBgSprite = "buttonclosepressed";
         CloseButton.relativePosition = new Vector3(this.width - CloseButton.width - inset, inset);
         CloseButton.eventClick += (component, param) => { ModCorral.mcButton.SimulateClick(); };

         DragHandle.target = ParentPanel;
         DragHandle.autoSize = true;
         DragHandle.height = this.height;
         DragHandle.width = this.width - 50;
         DragHandle.relativePosition = Vector3.zero;
         DragHandle.constrainToScreen = true;         
      }
   }
}
