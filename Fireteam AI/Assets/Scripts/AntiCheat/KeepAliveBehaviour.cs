namespace Koobando.AntiCheat
{
	using UnityEngine;
	using UnityEngine.SceneManagement;

	internal class ContainerHolder
	{
		public const string ContainerName = "Koobando Anti-Cheat";
		public static GameObject container;
	}

	public abstract class KeepAliveBehaviour<T> : MonoBehaviour where T: KeepAliveBehaviour<T>
	{
		protected int instancesInScene;

		#region static instance
		/// <summary>
		/// Allows reaching public properties from code.
		/// Can be null if behaviour does not exist in scene or if accessed at or before Awake phase.
		/// </summary>
		public static T Instance { get; protected set; }

		protected static T GetOrCreateInstance
		{
			get
			{
				if (Instance != null)
				{
					return Instance;
				}

				if (ContainerHolder.container == null)
				{
					ContainerHolder.container = new GameObject(ContainerHolder.ContainerName);
				}
				Instance = ContainerHolder.container.AddComponent<T>();
				return Instance;
			}
		}
		#endregion

		#region unity messages

		protected virtual void Awake()
		{
			instancesInScene++;
			if (Init(Instance, ""))
			{
				Instance = (T)this;
			}
		}

		protected virtual void Start()
		{
			if (ContainerHolder.container == null && gameObject.name == ContainerHolder.ContainerName)
			{
				ContainerHolder.container = gameObject;
			}
		}

		protected virtual void OnDestroy()
		{
			int componentsCount = GetComponentsInChildren<Component>().Length;
			if (transform.childCount == 0 && componentsCount <= 2)
			{
				Destroy(gameObject);
			}
			else if (name == ContainerHolder.ContainerName && componentsCount <= 2)
			{
				Destroy(gameObject);
			}

			instancesInScene--;

			if (Instance == this)
			{
				Instance = null;
			}
		}

		#endregion

		protected virtual bool Init(T instance, string detectorName)
		{
			if (instance != null && instance != this)
			{
				DisposeInternal();
				return false;
			}

			DontDestroyOnLoad(transform.parent != null ? transform.root.gameObject : gameObject);

			return true;
		}

		protected virtual void DisposeInternal()
		{
			Destroy(this);
		}
	}
}