using System;
using System.Collections.Generic;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;


namespace ModCorral
{

   public class ModCorral : IUserMod, ILoadingExtension
   {
      public static UIButton mcButton = null;
      public static ModCorralUI mcPanel = null;

      // constructor gets instantiated before any extensions are called
      public ModCorral()
      {
         // create our corral monobehaviour
         UIView uiv = UIView.GetAView();

         if (uiv != null && uiv.gameObject != null)
         {
            CorralRegistration creg = CorralRegistration.instance;
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
        
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "modcorral.oncreated");
      }

      public void OnLevelLoaded(LoadMode mode)
      {
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
                     DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "found maintoolstrip");

                     UIButton policiesButton = ts.Find<UIButton>("Policies"); // we use this as a template to get 'most' of what we need set up                
                     mcButton = ts.AddTab("ModCorral", policiesButton, false);
                     
                     if (mcButton != null)
                     {
                        mcButton.tooltip = "Open Mod Corral";
                        mcButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                        mcButton.scaleFactor = 0.6f; // to fit a little better when using options sprites
                        mcButton.normalFgSprite = "Options";
                        mcButton.hoveredFgSprite = "OptionsHovered";
                        mcButton.focusedFgSprite = "OptionsFocused";
                        mcButton.pressedFgSprite = "OptionsPressed";
                        mcButton.disabledFgSprite = "OptionsDisabled";
                        mcButton.eventClick += mcButton_eventClick;

                        UIPanel fscont = uiv.FindUIComponent<UIPanel>("FullScreenContainer");

                        if (fscont != null)
                        {
                           // create our ui panel
                           mcPanel = (ModCorralUI)fscont.AddUIComponent(typeof(ModCorralUI));
                        }
                        else
                           DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "no fullscreencontainer");

                        if (mcPanel != null)
                        {
                           mcPanel.transform.parent = fscont.transform;
                           mcPanel.initialize();
                           //mcPanel.anchor = UIAnchorStyle.All;

                           // add any buttons that might have been registered before we got created
                           if (CorralRegistration.RegisteredMods != null)
                           {
                              foreach (ModRegistrationInfo mri in CorralRegistration.RegisteredMods)
                              {
                                 if (mri.ModButton == null)
                                 {
                                    mri.ModButton = mcPanel.ScrollPanel.AddAButton(mri.ModName, mri.ButtonText, mri.HoverText);
                                 }
                              }
                           }

                           mcPanel.isVisible = false;
                        }
                     }
                  }
                  else
                  {
                     DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "failed to find maintoolstrip");
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
         }
      }

      public void OnLevelUnloading()
      {
      }

      public void OnReleased()
      {
      }
   }


   public class ModRegistrationInfo
   {
      public string ModName;
      public string ButtonText;
      public string HoverText;
      public UIButton ModButton;
      public Action<string> ClickCallback;
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
               //g_instance.name = "CorralRegistration";
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
      // [3] - string hover text
      // [4] - Action<string> delegate 
      //
      public void RegisterMod(object[] paramArray)
      {
         try
         {
            if (paramArray == null || paramArray.Length != 4)
            {
               return;
            }

            string p0 = paramArray[0] as string;
            string p1 = paramArray[1] as string;
            string p2 = paramArray[2] as string;
            Action<string> p3 = paramArray[3] as Action<string>;

            if (p0 == null || p1 == null || p3 == null) // these 3 are mandatory
            {
               return;
            }

            Register(p0, p1, p2, p3);

            ////temp
            //Register("one", "onetext", "one hover", null);
            //Register("two", "twotext", "two hover", null);
            ////temp
         }
         catch(Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, string.Format("Exception in CorralRegistration.RegisterMod(): {0}", ex.Message));
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
               return;
            }

            string p0 = paramArray[0] as string;
            string p1 = paramArray[1] as string;
            if (p0 == null || p1 == null) // all are mandatory
            {
               return;
            }

            DeRegister(p0, p1);

         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, string.Format("Exception in CorralRegistration.DeRegisterMod(): {0}", ex.Message));
         }
      }


      public bool Register(string modName, string buttonText, string hoverText, Action<string> callback)
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("CorralRegistration.Register()"));

         bool success = false;

         try
         {
            foreach(ModRegistrationInfo mri in RegisteredMods)
            {
               if (mri.ModName == modName && mri.ButtonText == buttonText)
               {
                  // already registered
                  DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("CorralRegistration.Register() - {0} already registered", modName));

                  return false;
               }                                           
            }

            // register it
            ModRegistrationInfo newMRI = new ModRegistrationInfo { ModName = modName, ButtonText = buttonText, HoverText = hoverText, ClickCallback = callback };
            
            // create new button...
            if (ModCorral.mcPanel != null)
            {
               newMRI.ModButton = ModCorral.mcPanel.ScrollPanel.AddAButton(modName, buttonText, hoverText);
            }

            RegisteredMods.Add(newMRI);
            success = true;

            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("CorralRegistration.Register() added mod: {0} {1}", newMRI.ModName, newMRI.ButtonText));
         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, string.Format("CorralRegistration.Register() threw an exception: {0}", ex.Message));
         }

         return success;
      }

      public bool DeRegister(string modName, string buttonText)
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("CorralRegistration.DeRegister()"));

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

               if (success && foundMRI.ModButton != null)
               {
                  if (ModCorral.mcPanel != null)
                  {
                     ModCorral.mcPanel.ScrollPanel.RemoveAButton(foundMRI.ModButton.name);
                     foundMRI.ModButton = null;
                  }                  
               }

               if (success)
               {
                  DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("CorralRegistration.Deregister() removed mod: {0} {1}", foundMRI.ModName, foundMRI.ButtonText));
               }
            }
         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, string.Format("CorralRegistration.Deregister() threw an exception: {0}", ex.Message));
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

         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Mod Corral is awake and listening for registrations.");
      }

      public void OnDestroy()
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Mod Corral onDestroy");
      }

   }
}
