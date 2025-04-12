using UnityEngine;
using UnityEngine.SceneManagement;

// Responsible for restarting our scene.

public class LevelManager : MonoBehaviour
{
    // Runtime
    PlayerControls pControls;

    [Header("Levels")]
    [SerializeField] GameObject[] levels;


    void Awake()
    {
        pControls = new PlayerControls();

        pControls.Menu.Restart.started += ctx => SceneManager.LoadScene(0);

        // Choose a random level.
        levels[Random.Range(0, levels.Length)].SetActive(true);
    }

#region Enable/Disable
    void OnEnable()
    {
        pControls.Enable();
    }

    void OnDisable()
    {
        pControls.Disable();
    }
#endregion
}
