﻿using System;
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
      public static void Message(string msg)
      {
         if (ModCorralConfig.instance.LogMessages)
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, msg);
      }
      public static void Warning(string msg)
      {
         if (ModCorralConfig.instance.LogWarnings)
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, msg);
      }
      public static void Error(string msg)
      {
         if (ModCorralConfig.instance.LogErrors)
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, msg);
      }
   }
}