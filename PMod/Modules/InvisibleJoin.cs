// Please don't use this it's dangerous af lol u r gonna get banned XD

//using MelonLoader;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Events;
//using Object = UnityEngine.Object;

//namespace PMod.Modules
//{
//    internal class InvisibleJoin : ModuleBase
//    {
//        internal bool IsInvisible = false;
//        internal MelonPreferences_Entry<bool> IsOn;

//        internal InvisibleJoin()
//        {
//            MelonPreferences.CreateCategory("InvisibleJoin", "PM - Invisible Join");
//            IsOn = MelonPreferences.CreateEntry("InvisibleJoin", "IsOn", false, "Activate Mod? This is a risky function. (Needs restart)");
//        }

//        internal override void OnUiManagerInit()
//        {
//            if (!IsOn.Value) return;
//            Transform ProgressPanel = GameObject.Find("UserInterface/MenuContent/Popups/LoadingPopup/ProgressPanel").transform;
//            Transform Parent_Loading_Progress = ProgressPanel.Find("Parent_Loading_Progress");
//            Transform GoButton = Parent_Loading_Progress.Find("GoButton");
//            Transform JoinButton = Object.Instantiate(GoButton, ProgressPanel);
//            Parent_Loading_Progress.localPosition = new Vector3(0, 17, 0);
//            JoinButton.localPosition = new Vector3(-2.4f, -124f, 0);
//            JoinButton.GetComponentInChildren<Text>().text = "Join Invisible";
//            JoinButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
//            JoinButton.GetComponent<Button>().onClick.AddListener((UnityAction)(() => 
//            { 
//                IsInvisible = true;
//                GoButton.GetComponent<Button>().onClick.Invoke();
//            }));
//        }
//    }
//}