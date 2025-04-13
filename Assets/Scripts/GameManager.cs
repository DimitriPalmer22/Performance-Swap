using UnityEngine;
using UnityEngine.Events;

// Responsible for maintaining our custom update loop.
// Custom update loop
// Win check
// On Win
// On Lose

public class GameManager : MonoBehaviour
{
    // Runtime
    [SerializeField] bool performUpdate = true;
    [SerializeField] bool hasWon = false;

    [Header("Update Loop")]
    [SerializeField] float updateInterval;
    [SerializeField] UnityEvent leftUpdateEvent;
    [SerializeField] UnityEvent rightUpdateEvent;
    [SerializeField] private float leftInterval;
    [SerializeField] private float rightInterval;

    [Header("Winning/Losing")]
    [SerializeField] Snake leftSnake;
    [SerializeField] Snake rightSnake;
    [SerializeField] GameObject winMenu;
    [SerializeField] GameObject loseMenu;
    [SerializeField] GameObject heart;


    void Start()
    {
        leftInterval = updateInterval / 2;
        rightInterval = updateInterval;
    }

    void Update()
    {
        // If interval hits 0, invoke update event and reset.
        if(performUpdate)
        {
            leftInterval -= Time.deltaTime;
            rightInterval -= Time.deltaTime;
        }

        if(leftInterval <= 0)
        {
            // Perform regular update functions.
            leftUpdateEvent.Invoke();

            // Reset interval.
            leftInterval = updateInterval;
        }

        if(rightInterval <= 0)
        {
            // Perform regular update functions.
            rightUpdateEvent.Invoke();

            // Reset interval.
            rightInterval = updateInterval;
        }

        // If both snakes have won, stop the game and display win screen!
        if(!hasWon && (leftSnake.GetHasWon() || rightSnake.GetHasWon()))
        {
            OnWin();
        }
    }

    // Stop our update loop.
    public void StopUpdateLoop()
    {
        performUpdate = false;
    }

    // Perform our win actions.
    public void OnWin()
    {
        StopUpdateLoop();

        winMenu.SetActive(true);

        // Set the heart position to the middle of the snakes.
        heart.transform.position = leftSnake.transform.position + new Vector3(0, 1, 0);
        heart.SetActive(true);
    }

    // Perform our lose actions.
    public void OnLose()
    {
        StopUpdateLoop();

        loseMenu.SetActive(true);
    }
}
