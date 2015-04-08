using System;
using System.Collections.Generic;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;


namespace ModCorral
{

   public class ModCorral : IUserMod, ILoadingExtension
   {
      public static ConfigDialog ModCorralConfigDialog = null;
      public static UIButton mcButton = null;
      public static ModCorralUI mcPanel = null;
      public static UIPanel TabPanel = null;
      public static float StartingAbsPosY = 0f;
      public static float StartingAbsPosX = 0f;

      // constructor gets instantiated before any extensions are called
      public ModCorral()
      {
         try
         {
            ModCorralConfig.SetInstance(ModCorralConfig.Deserialize("ModCorralConfig.xml"));

            // determine if we are enabled or not
            if (PluginManager.instance == null || PluginManager.instance.GetPluginsInfo() == null)
            {
               Log.Message("ModCorral quitting, PluginManager.instance is null.");
               return;
            }

            // the very first thing we need to do is check if we're enabled.  This constructor 
            // is called even if we're marked as not to be loaded, so if not enabled, don't do anything
            PluginManager.PluginInfo myPluginInfo = null;
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
               if (info.name == "ModCorral" || info.publishedFileID.AsUInt64 == 419090722)
               {
                  myPluginInfo = info;
                  break;
               }
            }

            if (myPluginInfo == null)
            {
               Log.Error("ModCorral PluginInfo not found, exiting.");

               return;
            }

            //// we need to be notified if our mod is enabled or disabled
            //PluginManager.instance.eventPluginsChanged += () => { this.EvaluateStatus(); };
            //PluginManager.instance.eventPluginsStateChanged += () => { this.EvaluateStatus(); };

            if (!myPluginInfo.isEnabled)
            {
               Log.Warning("ModCorral is disabled.");
               return;
            }

            Log.Message("ModCorral initializing.");

            // create our corral monobehaviour
            UIView uiv = UIView.GetAView();

            if (uiv != null && uiv.gameObject != null)
            {
               CorralRegistration creg = CorralRegistration.instance;
            }
         }
         catch(Exception ex)
         {
            Log.Error("ModCorral() Exception: " + ex.Message);
         }
      }

      public string Description
      {
          get { return "A tool to collect and display launch buttons for other mods."; }
      }

      public string Name
      {
         get { return "Mod Corral"; }
      }

      public void OnCreated(ILoading loading)
      {
      }

      public void OnLevelLoaded(LoadMode mode)
      {
         try
         {
            Log.Message("ModCorral.OnLevelLoaded() " + mode.ToString());

            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
               if (mcButton == null) // if not created yet
               {
                  // add button to the end of the TSBar MainToolstrip (UITabStrip) 
                  UIView uiv = UIView.GetAView();

                  if (uiv != null)
                  {
                     UITabstrip ts = uiv.FindUIComponent<UITabstrip>("MainToolstrip");

                     if (ts != null)
                     {
                        UIButton policiesButton = ts.Find<UIButton>("Policies"); // we use this as a template to get 'most' of what we need set up                
                        mcButton = ts.AddTab("ModCorral", policiesButton, false);

                        // find the panel added in the ts tabcontainer
                        foreach (UIComponent c in ts.tabContainer.components)
                        {
                           if (c.name.Contains("ModCorral"))
                           {
                              TabPanel = c as UIPanel;
                              c.clipChildren = false;
                              c.opacity = 0; // otherwise our panel gets clicks obscured when it's in the same place...

                              break;
                           }
                        }

                        // initial position info
                        StartingAbsPosY = ts.absolutePosition.y - 20; // get rid of hardcoded 20...

                        ts.eventSelectedIndexChanged += ts_eventSelectedIndexChanged;

                        if (mcButton != null)
                        {
                           StartingAbsPosX = mcButton.absolutePosition.x;

                           mcButton.tooltip = "Open Mod Corral";
                           mcButton.eventTooltipShow += (component, param) => { param.tooltip.relativePosition = new Vector3(param.tooltip.relativePosition.x + 25, param.tooltip.relativePosition.y + 10, param.tooltip.relativePosition.z); };
                           mcButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                           mcButton.scaleFactor = 0.6f; // to fit a little better when using options sprites
                           mcButton.normalFgSprite = "Options";
                           mcButton.hoveredFgSprite = "OptionsHovered";
                           mcButton.focusedFgSprite = "OptionsFocused";
                           mcButton.pressedFgSprite = "OptionsPressed";
                           mcButton.disabledFgSprite = "OptionsDisabled";
                           mcButton.eventClick += mcButton_eventClick;
                           mcButton.clipChildren = false;

                           UIPanel fscont = uiv.FindUIComponent<UIPanel>("FullScreenContainer");

                           if (fscont != null)
                           {
                              // create our ui panel
                              mcPanel = (ModCorralUI)fscont.AddUIComponent(typeof(ModCorralUI));
                           }
                           else
                              Log.Message("no fullscreencontainer");

                           if (mcPanel != null)
                           {
                              mcPanel.transform.parent = fscont.transform;
                              mcPanel.initialize();
                              //mcPanel.anchor = UIAnchorStyle.All;

                              mcPanel.isVisible = false;
                           }

                           ModCorralConfigDialog = (ConfigDialog)uiv.AddUIComponent(typeof(ConfigDialog));
                           ModCorralConfigDialog.ParentPanel = mcPanel;
                           ModCorralConfigDialog.isVisible = false;
                        }
                     }
                     else
                     {
                        Log.Message("failed to find maintoolstrip");
                     }
                  }
               }

               // add any buttons that might have been registered before we got created
               if (CorralRegistration.RegisteredMods != null)
               {
                  foreach (ModRegistrationInfo mri in CorralRegistration.RegisteredMods)
                  {
                     if (!mri.IsButtonInitialized())
                     {
                        if (mri is ToggleModRegistrationInfo)
                        {
                           ToggleModRegistrationInfo tmri = mri as ToggleModRegistrationInfo;

                           tmri.ToggleButton = mcPanel.ScrollPanel.AddAToggleButton(tmri);
                        }
                        else
                        {
                           mri.ModButton = mcPanel.ScrollPanel.AddAButton(mri);
                        }
                     }
                  }

                  UpdateNewCount();
               }

            }
         }
         catch (Exception ex)
         {
            Log.Error("ModCorral.OnLevelLoaded() Exception: " + ex.Message);
         }
      }

      public static void UpdateNewCount()
      {
         int count = CorralRegistration.RegisteredMods.Count; // total buttons now in list
         int lastcount = ModCorralConfig.instance.LastNumberModButtons;

         if ((count > lastcount) && mcButton != null)
         {
            // put red count in main button
            mcButton.text = string.Format("+{0}", count - lastcount);
            mcButton.textColor = Color.red;
            mcButton.hoveredTextColor = Color.red;
            mcButton.textScale = 0.85f;
            mcButton.textHorizontalAlignment = UIHorizontalAlignment.Right;
            mcButton.textVerticalAlignment = UIVerticalAlignment.Top;

            mcButton.tooltip = string.Format("Open Mod Corral ({0} new mods have registered)", count - lastcount);
         }
         else if (count < lastcount)
         {
            ModCorralConfig.instance.LastNumberModButtons = count;
            ModCorralConfig.Serialize("ModCorralConfig.xml", ModCorralConfig.instance);
         }
      }

      public static void ClearNewCount()
      {
         if (mcButton != null && !string.IsNullOrEmpty(mcButton.text))
         {
            mcButton.text = string.Empty;
            mcButton.tooltip = "Open Mod Corral";

            // save 
            ModCorralConfig.instance.LastNumberModButtons = CorralRegistration.RegisteredMods.Count;
            ModCorralConfig.Serialize("ModCorralConfig.xml", ModCorralConfig.instance);
         }
      }

      public void ts_eventSelectedIndexChanged(UIComponent component, int selectedIndex)
      {
         UITabstrip ts = component as UITabstrip;

         if (ts != null)
         {
            if (selectedIndex == -1)
            {
               if (mcPanel.isVisible)
               {
                  mcPanel.HideMe();
               }
            }
            else
            {
               UIButton button = ts.tabs[selectedIndex] as UIButton; // this should be a button

               if (button != null)
               {
                  if (button != mcButton && mcPanel.isVisible) // currently selected button is not us, but we're still visible...
                  {
                     mcPanel.HideMe();
                  }
               }
            }
         }
      }

      public void mcButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
      {
         if (mcPanel != null)
         {
            mcPanel.ShowMeHideMe();
            ClearNewCount();
         }
      }

      public void OnLevelUnloading()
      {
         Log.Message("ModCorral.OnLevelUnloading()");
      }

      public void OnReleased()
      {
      }
   }

   public class SpriteData
   {
      public string NormalFgSpritename;
      public Texture2D NormalFgTexture;
      public string NormalBgSpritename;
      public Texture2D NormalBgTexture;

      public string HoveredFgSpritename;
      public Texture2D HoveredFgTexture;
      public string HoveredBgSpritename;
      public Texture2D HoveredBgTexture;

      public string PressedFgSpritename;
      public Texture2D PressedFgTexture;
      public string PressedBgSpritename;
      public Texture2D PressedBgTexture;      
   }

   public class ModRegistrationInfo
   {
      public string ModName;
      public string ButtonText;
      public string HoverText;
      public UIButton ModButton;
      public Action<string> ClickCallback;

      public SpriteData SpriteInfo;

      public virtual bool IsButtonInitialized()
      {
         return ModButton != null;
      }

      public virtual void ClearButton()
      {
         ModButton = null;
      }

      public virtual string GetButtonText()
      {
         return ModButton == null ? null : ModButton.name;
      }
   }

   public class ToggleModRegistrationInfo : ModRegistrationInfo
   {
      public Action<string, int> ToggleCallback;
      public UIMultiStateButton ToggleButton;
      public SpriteData SpriteInfo_state2; // another full set of sprites for button state 2

      public override bool IsButtonInitialized()
      {
         return ToggleButton != null;
      }

      public override void ClearButton()
      {
         ToggleButton = null;
      }

      public override string GetButtonText()
      {
         return ToggleButton == null ? null : ToggleButton.name;
      }
   }

   public class CorralRegistration : MonoBehaviour
   {
      // singleton pattern
      private static CorralRegistration g_instance;

      public static CorralRegistration instance
      {
         get 
         {
            if (g_instance == null)
            {
               GameObject go = new GameObject();
               go.name = "CorralRegistrationGameObject";
               go.hideFlags = HideFlags.HideAndDontSave;

               g_instance = go.AddComponent<CorralRegistration>();
            }

            return g_instance;
         }
      }

      public static List<ModRegistrationInfo> RegisteredMods = new List<ModRegistrationInfo>();

      // SendMessage compatible method
      // takes one parameter, an array of objects
      // array must be of the form:
      // [0] - string name of mod
      // [1] - string button text
      // [2] - string hover text
      // [3] - Action<string> delegate 
      //
      // normal sprites, foreground and background
      //
      // [4] - optional normalFg spritename (either builtin or custom)
      // [5] - optional texture2d for normalFg spritename
      // [6] - optional normalBg spritename (either builtin or custom)
      // [7] - optional texture2d for normalBg spritename
      //
      // hovered sprites, foreground and background
      //
      // [8] - optional hoveredFg spritename (either builtin or custom)
      // [9] - optional texture2d for hoveredFg spritename
      // [10] - optional hoveredBg spritename (either builtin or custom)
      // [11] - optional texture2d for hoveredBg spritename
      //
      // pressed sprites, foreground and background
      //
      // [12] - optional pressedFg spritename (either builtin or custom)
      // [13] - optional texture2d for pressedFg spritename
      // [14] - optional pressedBg spritename (either builtin or custom)
      // [15] - optional texture2d for pressedBg spritename      
      public void RegisterMod(object[] paramArray)
      {
         try
         {
            if (paramArray == null || !(paramArray.Length == 4 || paramArray.Length == 6 || paramArray.Length == 16))
            {
               Log.Warning("ModCorral.RegisterMod() - Mandatory parameters null or number of parameters incorrect, skipping registration.");
               return;
            }

            string p0 = paramArray[0] as string;
            string p1 = paramArray[1] as string;
            string p2 = paramArray[2] as string;
            Action<string> p3 = paramArray[3] as Action<string>;

            if (p0 == null || p1 == null || p3 == null) // these 3 are mandatory
            {
               Log.Warning("ModCorral.RegisterMod() - Mandatory parameters not set, skipping registration.");
               return;
            }

            string p4 = null;
            Texture2D p5 = null;

            if (paramArray.Length >= 6)
            {
               p4 = paramArray[4] as string;
               p5 = paramArray[5] as Texture2D;

               if (p4 == null)
               {
                  Log.Warning("ModCorral.RegisterMod() - caller failed to specify normalFg spritename parameter, skipping registration.");
                  return;
               }
            }

            string p6 = null;
            Texture2D p7 = null;
            string p8 = null;
            Texture2D p9 = null;
            string p10 = null;
            Texture2D p11 = null;
            string p12 = null;
            Texture2D p13 = null;
            string p14 = null;
            Texture2D p15 = null;

            if (paramArray.Length == 16)
            {
               p6 = paramArray[6] as string;
               p7 = paramArray[7] as Texture2D;

               if (p6 == null)
               {
                  Log.Warning("ModCorral.RegisterMod() - caller failed to specify normalBg spritename parameter, skipping registration.");
                  return;
               }

               p8 = paramArray[8] as string;
               p9 = paramArray[9] as Texture2D;

               if (p8 == null)
               {
                  Log.Warning("ModCorral.RegisterMod() - caller failed to specify hoveredFg spritename parameter, skipping registration.");
                  return;
               }

               p10 = paramArray[10] as string;
               p11 = paramArray[11] as Texture2D;

               if (p10 == null)
               {
                  Log.Warning("ModCorral.RegisterMod() - caller failed to specify hoveredBg spritename parameter, skipping registration.");
                  return;
               }

               p12 = paramArray[12] as string;
               p13 = paramArray[13] as Texture2D;

               if (p12 == null)
               {
                  Log.Warning("ModCorral.RegisterMod() - caller failed to specify pressedFg spritename parameter, skipping registration.");
                  return;
               }

               p14 = paramArray[14] as string;
               p15 = paramArray[15] as Texture2D;

               if (p14 == null)
               {
                  Log.Warning("ModCorral.RegisterMod() - caller failed to specify pressedFg spritename parameter, skipping registration.");
                  return;
               }
            }

            Register(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
         }
         catch(Exception ex)
         {
            Log.Error(string.Format("CorralRegistration.RegisterMod() Exception: {0}", ex.Message));
         }
      }

      // SendMessage compatible method
      // takes one parameter, an array of objects
      // array must be of the form:
      // [0] - string name of mod
      // [1] - string button text
      // [2] - string hover text
      // [3] - Action<string, int> delegate.  string is modname+buttontext identifier, bool is current state flag (true=pressed, false=normal/unpressed)
      //
      // State 0 Sprites
      //--------------------
      // normal sprites, foreground and background
      //
      //     [4] - mandatory normalFg spritename (either builtin or custom)
      //     [5] - optional texture2d for normalFg spritename
      //     [6] - mandatory normalBg spritename (either builtin or custom)
      //     [7] - optional texture2d for normalBg spritename
      //
      //     hovered sprites, foreground and background
      //
      //     [8] - mandatory hoveredFg spritename (either builtin or custom)
      //     [9] - optional texture2d for hoveredFg spritename
      //     [10] - mandatory hoveredBg spritename (either builtin or custom)
      //     [11] - optional texture2d for hoveredBg spritename
      //
      //     pressed sprites, foreground and background
      //
      //     [12] - mandatory pressedFg spritename (either builtin or custom)
      //     [13] - optional texture2d for pressedFg spritename
      //     [14] - mandatory pressedBg spritename (either builtin or custom)
      //     [15] - optional texture2d for pressedBg spritename    
      //
      // State 1 Sprites
      //--------------------
      // normal sprites, foreground and background
      //
      //     [16] - mandatory normalFg spritename (either builtin or custom)
      //     [17] - optional texture2d for normalFg spritename
      //     [18] - mandatory normalBg spritename (either builtin or custom)
      //     [19] - optional texture2d for normalBg spritename
      //
      //     hovered sprites, foreground and background
      //
      //     [20] - mandatory hoveredFg spritename (either builtin or custom)
      //     [21] - optional texture2d for hoveredFg spritename
      //     [22] - mandatory hoveredBg spritename (either builtin or custom)
      //     [23] - optional texture2d for hoveredBg spritename
      //
      //     pressed sprites, foreground and background
      //
      //     [24] - mandatory pressedFg spritename (either builtin or custom)
      //     [25] - optional texture2d for pressedFg spritename
      //     [26] - mandatory pressedBg spritename (either builtin or custom)
      //     [27] - optional texture2d for pressedBg spritename    
      //
      public void RegisterModToggleButton(object[] paramArray)
      {
         try
         {
            if (paramArray == null)
            {
               Log.Warning("ModCorral.RegisterModToggleButton() - Mandatory parameters null, skipping registration.");
               return;
            }
            if (!(paramArray.Length == 16 || paramArray.Length == 28))
            {
               Log.Warning(string.Format("ModCorral.RegisterModToggleButton() - Number of parameters incorrect ({0}), skipping registration.", paramArray.Length));
               return;
            }

            string p0 = paramArray[0] as string;
            string p1 = paramArray[1] as string;
            string p2 = paramArray[2] as string;
            Action<string, int> p3 = paramArray[3] as Action<string, int>;

            if (p0 == null || p1 == null || p3 == null) // these 3 are mandatory
            {
               Log.Warning("ModCorral.RegisterModToggleButton() - Mandatory parameters not set, skipping registration.");
               return;
            }

            SpriteData spriteInfo = new SpriteData();

            if (paramArray.Length >= 16)
            {
               spriteInfo.NormalFgSpritename = paramArray[4] as string;
               spriteInfo.NormalFgTexture = paramArray[5] as Texture2D;

               spriteInfo.NormalBgSpritename = paramArray[6] as string;
               spriteInfo.NormalBgTexture = paramArray[7] as Texture2D;

               spriteInfo.HoveredFgSpritename = paramArray[8] as string;
               spriteInfo.HoveredFgTexture = paramArray[9] as Texture2D;

               spriteInfo.HoveredBgSpritename = paramArray[10] as string;
               spriteInfo.HoveredBgTexture = paramArray[11] as Texture2D;

               spriteInfo.PressedFgSpritename = paramArray[12] as string;
               spriteInfo.PressedFgTexture = paramArray[13] as Texture2D;

               spriteInfo.PressedBgSpritename = paramArray[14] as string;
               spriteInfo.PressedBgTexture = paramArray[15] as Texture2D;
            }

            SpriteData spriteInfo2 = new SpriteData();

            if (paramArray.Length >= 28)
            {
               spriteInfo2.NormalFgSpritename = paramArray[16] as string;
               spriteInfo2.NormalFgTexture = paramArray[17] as Texture2D;
               
               spriteInfo2.NormalBgSpritename = paramArray[18] as string;
               spriteInfo2.NormalBgTexture = paramArray[19] as Texture2D;

               spriteInfo2.HoveredFgSpritename = paramArray[20] as string;
               spriteInfo2.HoveredFgTexture = paramArray[21] as Texture2D;

               spriteInfo2.HoveredBgSpritename = paramArray[22] as string;
               spriteInfo2.HoveredBgTexture = paramArray[23] as Texture2D;

               spriteInfo2.PressedFgSpritename = paramArray[24] as string;
               spriteInfo2.PressedFgTexture = paramArray[25] as Texture2D;

               spriteInfo2.PressedBgSpritename = paramArray[26] as string;
               spriteInfo2.PressedBgTexture = paramArray[27] as Texture2D;
            }

            RegisterToggle(p0, p1, p2, p3, spriteInfo, spriteInfo2);
         }
         catch (Exception ex)
         {
            Log.Error(string.Format("CorralRegistration.RegisterModToggleButton() Exception: {0}", ex.Message));
         }
      }

      // SendMessage compatible method
      // takes one parameter, an array of objects
      // array must be of the form:
      // [0] - string name of mod
      // [1] - string button text
      //
      public void DeRegisterMod(object[] paramArray)
      {
         try
         {
            if (paramArray == null || paramArray.Length != 2)
            {
               Log.Warning("ModCorral.DeRegisterMod() - Mandatory parameter null or wrong size, skipping de-registration.");

               return;
            }

            string p0 = paramArray[0] as string;
            string p1 = paramArray[1] as string;
            if (p0 == null || p1 == null) // all are mandatory
            {
               Log.Warning("ModCorral.DeRegisterMod() - Mandatory parameters not set, skipping de-registration.");
               return;
            }

            DeRegister(p0, p1);

         }
         catch (Exception ex)
         {
            Log.Error(string.Format("CorralRegistration.DeRegisterMod() Exception: {0}", ex.Message));
         }
      }

      public bool Register(string modName, string buttonText, string hoverText, Action<string> callback, 
         string normalFgSpritename, Texture2D normalFgTexture
         )
      {
         return Register(modName, buttonText, hoverText, callback, normalFgSpritename, normalFgTexture,
            null, null, null, null, null, null, null, null, null, null);
      }

      public bool Register(string modName, string buttonText, string hoverText, Action<string> callback, 
         string normalFgSpritename, Texture2D normalFgTexture,
         string normalBgSpritename, Texture2D normalBgTexture,
         string hoveredFgSpritename, Texture2D hoveredFgTexture,
         string hoveredBgSpritename, Texture2D hoveredBgTexture,
         string pressedFgSpritename, Texture2D pressedFgTexture,
         string pressedBgSpritename, Texture2D pressedBgTexture
         )
      {
         Log.Message(string.Format("CorralRegistration.Register()"));

         bool success = false;

         try
         {
            foreach(ModRegistrationInfo mri in RegisteredMods)
            {
               if (mri.ModName == modName && mri.ButtonText == buttonText)
               {
                  // already registered
                  Log.Warning(string.Format("CorralRegistration.Register() - '{0}-{1}' already registered", modName, buttonText));

                  return false;
               }                                           
            }

            // register it
            ModRegistrationInfo newMRI = new ModRegistrationInfo
            {
               ModName = modName,
               ButtonText = buttonText,
               HoverText = hoverText,
               ClickCallback = callback,
               SpriteInfo = new SpriteData()
               {
                  NormalFgSpritename = normalFgSpritename,
                  NormalFgTexture = normalFgTexture,
                  NormalBgSpritename = normalBgSpritename,
                  NormalBgTexture = normalBgTexture,
                  HoveredFgSpritename = hoveredFgSpritename,
                  HoveredFgTexture = hoveredFgTexture,
                  HoveredBgSpritename = hoveredBgSpritename,
                  HoveredBgTexture = hoveredBgTexture,
                  PressedFgSpritename = pressedFgSpritename,
                  PressedFgTexture = pressedFgTexture,
                  PressedBgSpritename = pressedBgSpritename,
                  PressedBgTexture = pressedBgTexture
               }
            };
            
            // create new button...
            if (ModCorral.mcPanel != null)
            {
               newMRI.ModButton = ModCorral.mcPanel.ScrollPanel.AddAButton(newMRI);
               ModCorral.UpdateNewCount();
            }

            RegisteredMods.Add(newMRI);
            success = true;

            Log.Message(string.Format("CorralRegistration.Register() added mod: '{0}-{1}'", newMRI.ModName, newMRI.ButtonText));
         }
         catch (Exception ex)
         {
            Log.Error(string.Format("CorralRegistration.Register() Exception: {0}", ex.Message));
         }

         return success;
      }

      public bool RegisterToggle(string modName, string buttonText, string hoverText, Action<string, int> callback,
         SpriteData state0Sprites,
         SpriteData state1Sprites)
      {
         Log.Message(string.Format("CorralRegistration.RegisterToggle()"));

         bool success = false;

         try
         {
            foreach (ModRegistrationInfo mri in RegisteredMods)
            {
               if (mri.ModName == modName && mri.ButtonText == buttonText)
               {
                  // already registered
                  Log.Warning(string.Format("CorralRegistration.RegisterToggle() - '{0}-{1}' already registered", modName, buttonText));

                  return false;
               }
            }

            // register it
            ToggleModRegistrationInfo newMRI = new ToggleModRegistrationInfo
            {
               ModName = modName,
               ButtonText = buttonText,
               HoverText = hoverText,
               ToggleCallback = callback,
               SpriteInfo = state0Sprites,
               SpriteInfo_state2 = state1Sprites
            };

            // create new button...
            if (ModCorral.mcPanel != null)
            {
               newMRI.ToggleButton = ModCorral.mcPanel.ScrollPanel.AddAToggleButton(newMRI);

               ModCorral.UpdateNewCount();
            }

            RegisteredMods.Add(newMRI);
            success = true;

            Log.Message(string.Format("CorralRegistration.RegisterToggle() added mod: '{0}-{1}'", newMRI.ModName, newMRI.ButtonText));
         }
         catch (Exception ex)
         {
            Log.Error(string.Format("CorralRegistration.RegisterToggle() Exception: {0}", ex.Message));
         }

         return success;
      }

      public bool DeRegister(string modName, string buttonText)
      {
         Log.Message(string.Format("CorralRegistration.DeRegister()"));

         bool success = false;

         try
         {
            ModRegistrationInfo foundMRI = null;

            foreach (ModRegistrationInfo mri in RegisteredMods)
            {
               if (mri.ModName == modName && mri.ButtonText == buttonText)
               {
                  foundMRI = mri; // don't modify list while iterating, just to be safe
                  break;
               }
            }

            if (foundMRI != null)
            {
               // registered, so remove it and destroy UIButton
               success = RegisteredMods.Remove(foundMRI);

               if (success && !foundMRI.IsButtonInitialized())
               {
                  if (ModCorral.mcPanel != null)
                  {
                     ModCorral.mcPanel.ScrollPanel.RemoveAButton(foundMRI.GetButtonText());
                     foundMRI.ClearButton();
                  }                  
               }

               if (success)
               {
                  Log.Message(string.Format("CorralRegistration.Deregister() removed mod: '{0}-{1}'", foundMRI.ModName, foundMRI.ButtonText));
               }
            }
            else
            {
               Log.Warning(string.Format("CorralRegisration.Deregister() - failed to find mod '{0}-{1}' to de-register", modName, buttonText));
            }
         }
         catch (Exception ex)
         {
            Log.Error(string.Format("CorralRegistration.Deregister() Exception: {0}", ex.Message));
         }

         return success;
      }

      // Monobehaviour methods
      public void Awake()
      {
         DontDestroyOnLoad(this.gameObject);

         if (g_instance == null)
         {
            g_instance = this as CorralRegistration;
         }

         // add our config button
         CorralRegistration.instance.Register("ModCorral", "ModCorralConfig", "Open configuration for Mod Corral", (Action<string>)delegate(string s) { if (ModCorral.ModCorralConfigDialog != null) ModCorral.ModCorralConfigDialog.ShowOrHide(); },
            "Options", null,
            "OptionsBase", null,
            "OptionsHovered", null,
            "ToolbarIconGroup1Hovered", null, //"OptionsBaseHovered", null,
            "OptionsPressed", null,
            "ToolbarIconGroup1Pressed", null //"OptionsBasePressed", null
            ); 

         Log.Message("Mod Corral is awake and listening for registrations.");
      }

      public void OnDestroy()
      {
         Log.Message("Mod Corral onDestroy");
      }

   }
}
