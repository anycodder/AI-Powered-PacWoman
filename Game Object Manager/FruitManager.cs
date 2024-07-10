using UnityEngine;
using System.Collections;

public class FruitManager : MonoBehaviour
{
    public GameObject[] fruits; 
    public float initialDelay = 10f; 
    public float displayDuration = 5f; 
    public float displayInterval = 5f; 
    public GameManager gameManager { get; private set; }
    private int currentFruitIndex = 0; 

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        foreach (var fruit in fruits)
        {
            fruit.SetActive(false);  
        }
        StartCoroutine(ManageFruitsRoutine());
    }

    private IEnumerator ManageFruitsRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (gameManager.IsGameOver) // Check if the game is over
            {
                yield return null;
                continue;
            }

            GameObject currentFruit = fruits[currentFruitIndex];
            currentFruit.SetActive(true);  // Activate the fruit

            float elapsedTime = 0f;
            while (elapsedTime < displayDuration)
            {
                if (gameManager.Eaten) // Check if the game is over
                {
                    currentFruit.SetActive(false); // Deactivate fruit if game is over
                    yield return null;
                    continue;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (currentFruit.activeSelf)  // If the fruit is still active (not eaten)
            {
                currentFruit.SetActive(false);  // Deactivate the fruit
            }

            currentFruitIndex = (currentFruitIndex + 1) % fruits.Length;  // Update index for the next fruit

            yield return new WaitForSeconds(displayInterval);  // Wait for the interval before showing the next fruit
        }
    }
}
