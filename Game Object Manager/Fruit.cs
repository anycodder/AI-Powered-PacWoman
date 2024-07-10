using System.Data;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public int points = 500; 
    public GameManager gameManager;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Pacwoman")) // Eğer çarpışan obje Pacwoman ise
        {
            Debug.Log("yedi mi ?");
            Eat();
        }
    }

    private void Eat()
    {
        Debug.Log("evet");
        FindObjectOfType<GameManager>().FruitEaten(this);
    }
    
}