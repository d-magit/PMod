using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Client.Functions
{
    public class EmmAllower
    {
        public static void OnSceneWasLoaded()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (GameObject rootGameObject in activeScene.GetRootGameObjects())
            {
                if (rootGameObject.name == "eVRCRiskFuncDisable")
                {
                    Object.DestroyImmediate(rootGameObject);
                }
            }
            SceneManager.MoveGameObjectToScene(new GameObject("eVRCRiskFuncEnable"), activeScene);
        }
    }
}