using UnityEngine;

interface ISceneComponent
{
	void Initialize(MainScene scene);
}

sealed class MainScene : MonoBehaviour
{
	// PUBLIC MEMBERS

	public PlayerController    PlayerController { get; private set; }
	public InputManager        InputManager     { get; private set; }
	public BlockTerrainManager TerrainManager { get; private set; }

	// MONOBEHAVIOUR INTERFACE

	private void Awake()
	{
		InitializeSceneComponents();

		SetPlayerPosition();

		InputManager.QuickSave += OnQuickSave;
	}

	// HANLDERS

	private void OnQuickSave()
	{
		Debug.Log("TODO QuickSave");
	}

	// PRIVATE METHODS

	private void InitializeSceneComponents()
	{
		var sceneComponents = GetComponentsInChildren<ISceneComponent>(true);

		PlayerController = FindComponent<PlayerController   >(sceneComponents);
		InputManager     = FindComponent<InputManager       >(sceneComponents);
		TerrainManager   = FindComponent<BlockTerrainManager>(sceneComponents);

		System.Array.ForEach(sceneComponents, component => component.Initialize(this));
	}

	private void SetPlayerPosition()
	{
		PlayerController.transform.position = new Vector3(TerrainManager.ChunkWidth / 2, TerrainManager.ChunkHeight + 10, TerrainManager.ChunkWidth / 2);
	}

	// HELPERS

	private static T FindComponent<T>(ISceneComponent[] components) where T : Component
	{
		return System.Array.Find(components, component => component is T) as T;
	}
}
