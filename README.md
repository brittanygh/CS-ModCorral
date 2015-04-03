# CS-ModCorral

Mod Corral - a mod for other mods to use

Mod Corral is a pop-out panel that holds simple buttons for other mods.  It's designed to be small and focused, and doesn't provide any additional functionality.  It is useful for mods that have one or two buttons to activate or configure them, and is designed to help reduce on screen clutter and overlap of buttons created by mods.

It adds a button in the game tab strip, right next to the policies button (see screenshots).  It adds itself to the tabs collection and behaves as if it were a standard control.

Clicking it slides up a scrollable panel with buttons that are dynamically added on requests from other mods.  The list of buttons is vertically layed out and scrollable.  Clicking a button fires a callback to allow the other mod to do whatever it wants, and then the Mod Corral panel slides shut.

As a beta, I'm looking for feedback on features - but keep in mind I want it to be small, not to grow and try to do everything for everyone.

Some issues or features I'll fix or add:
- Advisor/tutorial panel doesn't recognize ModCorral panel
- resizeable panel
- persist size/location of panel, implement ModCorral configuration dialog as needed
- more control over size of buttons for mods?

Source is up at: https://github.com/brittanygh/CS-ModCorral 
Steam workshop link: 

Technical notes for implemenation:
- ModCorral expects mods to register via SendMessage, passing a method name and an array of params
- methods names must be: "RegisterMod" or "DeRegisterMod" only
- RegisterMod expects param array of this form:
	[0] string (name of mod)
	[1] string (name of button, if no sprite name specified, button will use this text)
	[2] string (tooltip hover text for button)
	[3] Action<string> callback delegate
	[4] optional param, string (built-in sprite name for button, must be Colossal sprite name unless you specify texture, in which case any string will do)
	[5] optional param, Texture2D (image for button, if specified, [4] custom sprite name is required)

- DeRegisterMod expects param array of this form:
	[0] string (name of mod)
	[1] string (name of button)

- you need to call register just once, either in OnLevelLoaded() of ILoadingExtension, or Awake() of a MonoBehaviour
- you should de-register, but button list is cleaned up when level is unloaded
- only active in LoadGame or NewGame (not asset or map editor)
- hovering over buttons uses green hovercolor to indicate 'focus'.  I could provide the ability to specify multiple sprites (and multiple textures) for foreground/background, hover/focus, etc.  More work for modders, but would look better

Example:

   public class MyMonoB : MonoBehaviour
   {
      public static GameObject corralGo = null;

      public void Awake()
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "ChirpBanner in Awake()");

         // find modcorral
         corralGo = GameObject.Find("CorralRegistrationGameObject");

         if (corralGo != null)
         {
            Action<string> callbackDel = this.ModCorralClickCallback;
            object[] paramArray = new object[6];

            paramArray[0] = "ChirpyBannerMod";
            paramArray[1] = "Chirpy Config";
            paramArray[2] = "Open the configuration panel for Chirpy Banner";
            paramArray[3] = callbackDel;
            paramArray[4] = "ChirperIcon"; // Colossal built-in sprite name
            paramArray[5] = null;


            corralGo.SendMessage("RegisterMod", paramArray);
         }
         else
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "ChirpBanner cound not find corral gameobject");
         }
      }

      public void ModCorralClickCallback(string buttonName)
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Chirpy Banner got a callback from mod corral!  Button text is: " + buttonName);
         ChirpyBanner.theBannerConfigPanel.ShowPanel(Vector2.zero); // chirpy banner code to bring up config dialog
      }
   }

