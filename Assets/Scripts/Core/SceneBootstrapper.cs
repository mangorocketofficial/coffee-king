using UnityEngine;
using UnityEngine.SceneManagement;
using CoffeeKing.Util;

namespace CoffeeKing.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        private static SceneBootstrapper instance;
        private static bool runtimeBootstrapCreated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntimeBootstrapper()
        {
            if (runtimeBootstrapCreated || FindAnyObjectByType<SceneBootstrapper>() != null)
            {
                return;
            }

            runtimeBootstrapCreated = true;
            var bootstrapperObject = new GameObject("[Bootstrapper]");
            bootstrapperObject.AddComponent<SceneBootstrapper>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Input.multiTouchEnabled = true;
            SpriteFactory.ClearCache();

            EnsureGameManager();
        }

        private void Start()
        {
            ConfigureMainCamera();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            instance = null;
            runtimeBootstrapCreated = false;
        }

        private static void EnsureGameManager()
        {
            if (FindAnyObjectByType<GameManager>() != null)
            {
                return;
            }

            var managerObject = new GameObject("[GameManager]");
            DontDestroyOnLoad(managerObject);
            managerObject.AddComponent<GameManager>();
        }

        private static void ConfigureMainCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null && Camera.allCamerasCount > 0)
            {
                mainCamera = Camera.allCameras[0];
            }

            if (mainCamera == null)
            {
                return;
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            ConfigureMainCamera();
            EnsureGameManager();
        }
    }
}
