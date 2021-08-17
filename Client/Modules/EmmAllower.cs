using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Client.Modules
{
    internal class EmmAllower : ModuleBase
    {
        internal override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (GameObject rootGameObject in activeScene.GetRootGameObjects())
                if (rootGameObject.name == "eVRCRiskFuncDisable")
                    Object.DestroyImmediate(rootGameObject);
            SceneManager.MoveGameObjectToScene(new GameObject("eVRCRiskFuncEnable"), activeScene);
        }
    }
}