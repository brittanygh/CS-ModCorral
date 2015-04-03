using System;
using System.Collections.Generic;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace ModCorral
{
   public static class Log
   {
      public static bool DoInfos = false;
      public static bool DoErrors = true;

      public static void Message(string msg)
      {
         if (DoInfos)
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, msg);
      }
      public static void Warning(string msg)
      {
         if (DoInfos)
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, msg);
      }
      public static void Error(string msg)
      {
         if (DoErrors)
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, msg);
      }
   }
}