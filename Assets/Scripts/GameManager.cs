using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Responsible for maintaining our custom update loop.
// Custom update loop
// Win check
// On Win
// On Lose

public class GameManager : MonoBehaviour
{
    // Instance for singleton pattern
    public static GameManager Instance { get; private set; }

    // Runtime
    [SerializeField] private bool performUpdate = true;
    [SerializeField] private bool hasWon = false;

    [Header("Update Loop"), SerializeField]
    private float updateInterval;

    [SerializeField] private UnityEvent leftUpdateEvent;
    [SerializeField] private UnityEvent rightUpdateEvent;
    [SerializeField] private float leftInterval;
    [SerializeField] private float rightInterval;

    [Header("Winning/Losing"), SerializeField]
    private Snake leftSnake;

    [SerializeField] private Snake rightSnake;
    [SerializeField] private GameObject winMenu;
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject heart;

    public bool IsGameOver => !performUpdate;

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Set the intervals
        leftInterval = updateInterval / 2;
        rightInterval = updateInterval;
    }

    private void Start()
    {
        // Start coroutines for each snake
        StartCoroutine(UpdateSnakeCoroutine(false));
        StartCoroutine(UpdateSnakeCoroutine(true));
    }

    private IEnumerator UpdateSnakeCoroutine(bool isRight)
    {
        // Delay the start of the updates depending on the direction of the snake
        if (isRight)
            yield return new WaitForSeconds(rightInterval);
        else
            yield return new WaitForSeconds(leftInterval);

        // Loop until we stop the game.
        while (performUpdate)
        {
            // Perform the update event.
            if (isRight)
                rightUpdateEvent.Invoke();
            else
                leftUpdateEvent.Invoke();

            // If both snakes have won, stop the game and display win screen!
            if (!hasWon && (leftSnake.GetHasWon() || rightSnake.GetHasWon()))
                OnWin();

            // Wait for the next interval.
            yield return new WaitForSeconds(updateInterval);
        }
    }

    // Stop our update loop.
    private void StopUpdateLoop()
    {
        performUpdate = false;
    }

    // Perform our win actions.
    private void OnWin()
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