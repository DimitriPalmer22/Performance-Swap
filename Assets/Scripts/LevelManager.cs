using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Responsible for restarting our scene.

public class LevelManager : MonoBehaviour
{
    // Runtime
    private PlayerControls _pControls;

    [Header("Levels"), SerializeField] private GameObject[] levels;

    private void Awake()
    {
        _pControls = new PlayerControls();
        _pControls.Menu.Restart.started += OnRestart;

        // Choose a random level.
        levels[Random.Range(0, levels.Length)].SetActive(true);
    }

    private void OnRestart(InputAction.CallbackContext ctx)
    {
        // Reload the current scene
        SceneManager.LoadScene(0);
    }

    #region Enable/Disable

    private void OnEnable()
    {
        _pControls.Enable();
    }

    private void OnDisable()
    {
        _pControls.Disable();
    }

    #endregion
}