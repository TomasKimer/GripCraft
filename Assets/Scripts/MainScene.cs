using UnityEngine;

interface ISceneComponent
{
	void Initialize(MainScene scene);
}

sealed class MainScene : MonoBehaviour
{
	// PUBLIC MEMBERS

	public PlayerController PlayerController { get; private set; }
	public InputManager     InputManager     { get; private set; }

	// MONOBEHAVIOUR INTERFACE

	private void Awake()
	{
		var sceneComponents = GetComponentsInChildren<ISceneComponent>(true);

		PlayerController = FindComponent<PlayerController>(sceneComponents);
		InputManager     = FindComponent<InputManager    >(sceneComponents);

		System.Array.ForEach(sceneComponents, component => component.Initialize(this));
	}

	// HELPERS

	private static T FindComponent<T>(ISceneComponent[] components) where T : Component
	{
		return System.Array.Find(components, component => component is T) as T;
	}
}
