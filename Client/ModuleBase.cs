using UnityEngine;
using VRC;
using VRC.Core;

namespace Client
{
	internal abstract class ModuleBase
	{
		internal virtual void OnUiManagerInit() { }

		internal virtual void OnPreferencesSaved() { }

		internal virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }

		internal virtual void OnUpdate() { }

		internal virtual void OnPlayerJoined(Player player) { }

		internal virtual void OnPlayerLeft(Player player) { }

		internal virtual void OnInstanceChange(ApiWorld world, ApiWorldInstance instance) { }

		internal virtual void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager) { }
	}
}