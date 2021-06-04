using Client.Functions.Utils;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRC;
using Object = UnityEngine.Object;

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
            {
                Timer timer = new();
                EntryDict.Add(id, timer);
                var text = player.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Text Container/Sub Text").gameObject;
                timer.text = Object.Instantiate(text, text.transform.parent);
                Object.DestroyImmediate(timer.text.transform.Find("Icon").gameObject);
                timer.text.transform.SetSiblingIndex(1);
                var TM = timer.text.GetComponentInChildren<TextMeshProUGUI>();
                TM.text = "Frozen ";
                TM.color = Color.cyan;
                timer.text.SetActive(false);
            }
        }

        private static void OnLeave(Player player) => EntryDict.Remove(player.prop_APIUser_0.id);

        public static void NametagSet(Timer entry)
        {
            try
            {
                var rect = entry.text.transform.GetComponent<RectTransform>();
                if (entry.IsFrozen)
                {
                    rect.sizeDelta = new(rect.sizeDelta.x, rect.sizeDelta.y - 20);
                    entry.text?.SetActive(true);
                }
                else
                {
                    rect.sizeDelta = new(rect.sizeDelta.x, rect.sizeDelta.y + 20);
                    entry.text?.SetActive(false);
                }
            }
            catch { }
        }
    }
}