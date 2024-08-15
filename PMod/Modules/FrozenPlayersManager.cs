using PMod.Utils;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRC;
using Object = UnityEngine.Object;

namespace PMod.Modules
{
    internal class FrozenPlayersManager : ModuleBase
    {
        internal Dictionary<string, Timer> EntryDict = new();

        internal override void OnPlayerJoined(Player player)
        {
            var id = player.prop_APIUser_0.id;
            if (id != Player.prop_Player_0.prop_APIUser_0.id)
            {
                Timer timer = new();
                EntryDict.Add(id, timer);
                var text = player.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Text Container/Sub Text").gameObject;
                timer.text = Object.Instantiate(text, text.transform.parent);
                Object.DestroyImmediate(timer.text.transform.Find("Icon").gameObject);
                var TM = timer.text.GetComponentInChildren<TextMeshProUGUI>();
                TM.text = "Frozen";
                TM.color = Color.cyan;
                timer.text.SetActive(false);
            }
        }

        internal override void OnPlayerLeft(Player player) => EntryDict.Remove(player.prop_APIUser_0.id);

        internal void NametagSet(Timer entry)
        {
            try
            {
                var rect = entry.text.transform.GetComponent<RectTransform>();
                if (entry.IsFrozen)
                    entry.text?.SetActive(true);
                else
                    entry.text?.SetActive(false);
            }
            catch { }
        }
    }
}