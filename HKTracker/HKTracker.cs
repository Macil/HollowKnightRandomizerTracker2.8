﻿using System.Collections.Generic;
using System.Diagnostics;
using Modding;
using WebSocketSharp.Server;
using System.Reflection;

namespace HKTracker
{

    /// <summary>
    /// Main mod class for PlayerDataDump.  Provides the server and version handling.
    /// </summary>
    public class HKTracker : Mod, IMenuMod, IGlobalSettings<GlobalSettings>
    {
        public static GlobalSettings GS = new GlobalSettings();
        public GlobalSettings OnSaveGlobal() => GS;
        public void OnLoadGlobal(GlobalSettings s) => GS = s;
        public override int LoadPriority() => 9999;
        private readonly WebSocketServer _wss = new WebSocketServer(11420);
        /// <summary>
        /// Used by websocket OnMessage callbacks to run tasks on the main game thread.
        /// </summary>
        internal UnityMainThreadTaskScheduler mainThreadScheduler;
        readonly string[] StyleValues = new string[] { "Classic", "Modern" };
        readonly string[] ColorValues = new string[] { "Default", "Red", "Green", "Blue", "Crimson", "Dark Red", "Pink", "Light Pink", "Hot Pink", "Orange", "Dark Orange", "Yellow", "Gold", "Purple", "Medium Purple", "Indigo", "Lime", "Chartreuse", "Yellow Green", "Turqoise", "Steel Blue", "Navy" };
        internal static HKTracker Instance;

        public bool ToggleButtonInsideMenu => true;

        /// <summary>
        /// Fetches the list of the current mods installed.
        /// </summary>
        public override string GetVersion() => FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(HKTracker)).Location).FileVersion;
        /// <summary>
        /// Creates and starts the WebSocket Server instances.
        /// </summary>
        public override void Initialize()
        {
            Instance = this;
            mainThreadScheduler = new UnityMainThreadTaskScheduler();

            Log("Initializing PlayerDataDump");
            //Setup websockets server
            _wss.AddWebSocketService<SocketServer>("/playerData");

            //Setup ProfileStorage Server
            _wss.AddWebSocketService<ProfileStorageServer>("/ProfileStorage");

            _wss.Start();
            Log("Initialized PlayerDataDump");

        }
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry
                {
                    Name = "Style",
                    Description = null,
                    Values = StyleValues,
                    Saver = opt => GS.TrackerStyle = (GlobalSettings.Style)opt,
                    Loader = () => (int)GS.TrackerStyle
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Border Glow",
                    Description = "Enable or Disable Border Glow when using Modern Style",
                    Values = new string []
                    {
                        "On",
                        "Off"
                    },
                    Saver = opt => GS.TrackerGlow = (GlobalSettings.BorderGlow)opt,
                    Loader = () => (int)GS.TrackerGlow
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Equip Item Color",
                    Description = null,
                    Values = ColorValues,
                    Saver = opt => GS.EquipColor = (GlobalSettings.Color)opt,
                    Loader = () => (int)GS.EquipColor
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Used Key Color",
                    Description= null,
                    Values = ColorValues,
                    Saver= opt => GS.GaveColor = (GlobalSettings.Color)opt,
                    Loader = () => (int)GS.GaveColor
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Presets",
                    Description = "Only works with OBS and browser source set to kingkiller39.github.io/HollowKnightRandomizerTracker2.8",
                    Values = new string []
                    {
                        "Player Custom 1",
                        "Player Custom 2",
                        "Player Custom 3",
                        "Everything",
                        "Minimal Left",
                        "Minimal Right",
                        "Rando Racing"
                    },

                    Saver = opt => GS.TrackerProfile = (GlobalSettings.Profile)opt,
                    Loader = () => (int)GS.TrackerProfile
                }
            };
        }

        /// <summary>
        /// Called when the mod is disabled, stops the web socket server and removes the socket services.
        /// </summary>
        public void Unload()
        {
            _wss.Stop();
            _wss.RemoveWebSocketService("/playerData");
            _wss.RemoveWebSocketService("/ProfileStorage");
            mainThreadScheduler.Dispose();
        }
    }
}
