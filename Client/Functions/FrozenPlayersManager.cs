using Client.Functions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using VRC;
using VRC.Core;

namespace Client.Functions
{
    internal static class FrozenPlayersManager
    {
        public static Dictionary<string, Timer> EntryDict = new();

        public static void OnApplicationStart()
        {
            NetworkEvents.OnJoin += OnJoin;
            NetworkEvents.OnLeave += OnLeave;
        }

        private static void OnJoin(Player player)
        {
            var id = player.prop_APIUser_0.id;
            if (id != Player.prop_Player_0.prop_APIUser_0.id)
                EntryDict.Add(id, new Timer());
        }

        private static void OnLeave(Player player) => EntryDict.Remove(player.prop_APIUser_0.id);

        public static void NametagSet(Timer entry)
        {
            APIUser User = Utilities.GetPlayerFromID(EntryDict.FirstOrDefault(x => x.Value == entry).Key)?.prop_APIUser_0;
            if (User == null) return;

            if (entry.IsFrozen)
            {
                MelonLogger.Msg(ConsoleColor.Red, $"Warning! Detected frozen player: {User.displayName}");
            }
            else MelonLogger.Msg(ConsoleColor.Green, $"Player {User.displayName} unfroze.");
        }
    }
}