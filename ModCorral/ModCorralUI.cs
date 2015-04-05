using System;
using System.Collections.Generic;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace ModCorral
{
   public class ModCorralUI : UIPanel
   {
      public static float m_SafeMargin = 20f;
      public static float m_ShowHideTime = 0.3f;

      public static int GeneralWidth = 75;
      public static int GeneralHeight = 540; // 10 buttons at 50 each plus top and bottom padding of 2 each

      public bool m_Initialized = false;
      public bool isHiding = false;

      public Vector3 normalDisplayRelPos = Vector3.zero;

      //public UITitleSubPanel TitleSubPanel;
      public UIScrollButtonPanel ScrollPanel;

      //public Vector3 SavedCloseButtonPos;
      //public UIButton CloseToolbarButton;

      public AudioClip m_SwooshInSound;
      public AudioClip m_SwooshOutSound;
      
      public void initialize()
      {
         float viewWidth = UIView.GetAView().GetScreenResolution().x;
         float viewHeight = UIView.GetAView().GetScreenResolution().y;

         this.size = new Vector2(GeneralWidth, GeneralHeight);
         this.width = GeneralWidth;
         this.height = GeneralHeight;

         this.absolutePosition = new Vector3(ModCorral.StartingAbsPosX, ModCorral.StartingAbsPosY - GeneralHeight, 0);
         this.normalDisplayRelPos = this.relativePosition;//new Vector3(viewWidth  - this.size.x, viewHeight - this.size.y, 0f);
         //this.relativePosition = normalDisplayRelPos;

         this.backgroundSprite = "SubcategoriesPanel";

         this.clipChildren = false;
         this.canFocus = true;
         this.isInteractive = true;
        // this.eventPositionChanged += (component, eventParam) => { if (!isHiding && isVisible) this.CloseToolbarButton.absolutePosition = this.TitleSubPanel.CloseButton.absolutePosition; };

         float inset = 0;// 5f;

         //// title bar and close button
         //TitleSubPanel = AddUIComponent<UITitleSubPanel>();
         //TitleSubPanel.ParentPanel = this;
         //TitleSubPanel.relativePosition = new Vector3(inset, inset, 0);
         //TitleSubPanel.width = this.width;
         //TitleSubPanel.height = 30;

         ScrollPanel = AddUIComponent<UIScrollButtonPanel>();
         ScrollPanel.ParentPanel = this;
         ScrollPanel.relativePosition = new Vector3(0, 0, 0);//new Vector3(inset, TitleSubPanel.relativePosition.y + TitleSubPanel.height + inset);
         ScrollPanel.width = this.width;
         ScrollPanel.height = this.height - ScrollPanel.relativePosition.y - inset;

         PoliciesPanel pp = UIView.GetAView().GetComponentInChildren<PoliciesPanel>();

         if (pp != null)
         {
            m_SwooshInSound = pp.m_SwooshInSound;
            m_SwooshOutSound = pp.m_SwooshOutSound;
         }

         m_Initialized = true;
      }

      public override void Start()
      {
         //SavedCloseButtonPos = TitleSubPanel.CloseButton.absolutePosition;
         //CloseToolbarButton = UIView.Find<UIButton>("TSCloseButton");

         //test
         //CloseToolbarButton.eventClick += CloseToolbarButton_eventClick;
         base.Start();
      }

      public void CloseToolbarButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
      {
         if (isVisible)
         {
            HideMe();
         }
      }

      public void ShowMeHideMe()
      {
         if (!this.isVisible)
         {
            ShowMe();
         }
         else
         {
            HideMe();
         }
      }
     
      public void ShowMe()
      {
         if (!m_Initialized)// || isVisible)
         {
            return;
         }

         isHiding = false;

         //TitleSubPanel.CloseButton.Hide();         

         // pop in from  below
         float num = this.normalDisplayRelPos.y + this.size.y;
         float end = num - size.y;
         float start = num + m_SafeMargin;

         this.Show();

         //Log.Message("mcbutton state: " + ModCorral.mcButton.state.ToString());

         //if (ModCorral.mcButton.state != UIButton.ButtonState.Pressed)
         //{
         //   ModCorral.mcButton.state = UIButton.ButtonState.Pressed;
         //}
         //Log.Message("mcbutton state: " + ModCorral.mcButton.state.ToString());

         ValueAnimator.Animate(this.GetType().ToString(), (Action<float>)(val =>
         {
            Vector3 relativePosition = this.relativePosition;
            relativePosition.y = val;
            relativePosition.x = this.normalDisplayRelPos.x;
            this.relativePosition = relativePosition;
            //this.CloseToolbarButton.absolutePosition = this.TitleSubPanel.CloseButton.absolutePosition;
         }), new AnimatedFloat(start, end, m_ShowHideTime, EasingType.ExpoEaseOut));

         Singleton<AudioManager>.instance.PlaySound(this.m_SwooshInSound, 1f);

      }

      public void HideMe()
      {
         if (!m_Initialized)// || !isVisible)
         {
            return;
         }

         isHiding = true;

         // hide by moving down
         float num = this.normalDisplayRelPos.y + this.size.y;
         float start = num - size.y;
         float end = num + m_SafeMargin;

         //Log.Message("mcbutton state: " + ModCorral.mcButton.state.ToString());
         //if (ModCorral.mcButton.state != UIButton.ButtonState.Normal)
         //{
         //   ModCorral.mcButton.state = UIButton.ButtonState.Normal;
         //   ModCorral.mcButton.Invalidate();
         //   ModCorral.mcButton.Update();
            
         //}
         //Log.Message("mcbutton state: " + ModCorral.mcButton.state.ToString());

         ValueAnimator.Animate(this.GetType().ToString(), (Action<float>)(val =>
         {
            Vector3 relativePosition = this.relativePosition;
            relativePosition.y = val;
            relativePosition.x = this.normalDisplayRelPos.x;
            this.relativePosition = relativePosition;
         }), new AnimatedFloat(start, end, m_ShowHideTime, EasingType.ExpoEaseOut), (Action)(() => this.Hide()));

         //this.TitleSubPanel.CloseButton.Show();
         //this.CloseToolbarButton.absolutePosition = this.SavedCloseButtonPos;

         //if (!this.component.isVisible || !this.m_EnableAudio || (!Singleton<AudioManager>.exists || !((UnityEngine.Object)this.m_SwooshOutSound != (UnityEngine.Object)null)))
         //   return;
         Singleton<AudioManager>.instance.PlaySound(this.m_SwooshOutSound, 1f);
      }

   }
}
