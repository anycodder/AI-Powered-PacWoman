using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Ghost[] ghosts;
    [SerializeField] public Pacwoman pacwoman;
    [SerializeField] public Transform nodes;
    [SerializeField] public Transform pellets;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI gameWinText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI userScoreText;
    public bool Eaten = false;

    public Button minimaxButton; // Reference to the Minimax button
    public Button alphaBetaButton; // Reference to the Alpha-Beta button
    public Button expectimaxButton; // Reference to the Expectimax button
    public Button userButton; // Reference to the User button
    public FruitManager fruitManager;
    public bool isPacmanInvincible = false;
    public bool IsGameOver = false;
    public bool gameOverr = false;
    public bool restart = true;
    public bool x = false;
    public List<Vector2> pelletPositions = new List<Vector2>();
    public List<Vector2> nodePositions = new List<Vector2>();
    public AnimationOfPacwoman animationOfPacwoman { get; private set; }
    public Movement movement {get; private set; }
    public AnimatedSpriteforDeathforPacwoman deathanimeted { get; private set; }
    public GhostHome ghostHome { get; private set; }
    public GeneticAlgorithm geneticAlg { get; private set; }
    public Functions func { get; private set; }

    private int ghostMultiplier = 1;
    public string playerName;
    public int lives = 3;
    public int score = 0;
    public int lastscore = 0;

    private List<string> player_data = new List<string>();
    private string filePath;
    private string sceneName;

    public int Lives => lives;
    public int Score => score;

    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public int currentMunch = 0;

    public AudioSource death;
    public AudioSource startGameAuido;
    public AudioSource powerPelletaudio;
    public AudioSource ghostEatenAudio;
    public AudioSource fruitEatenAudio;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        siren.Play();
        ghostHome = FindObjectOfType<GhostHome>();
        animationOfPacwoman = FindObjectOfType<AnimationOfPacwoman>();
        deathanimeted = FindObjectOfType<AnimatedSpriteforDeathforPacwoman>();
        geneticAlg = FindObjectOfType<GeneticAlgorithm>();
        func = FindObjectOfType<Functions>();

        minimaxButton.gameObject.SetActive(false);
        alphaBetaButton.gameObject.SetActive(false);
        expectimaxButton.gameObject.SetActive(false);
        userButton.gameObject.SetActive(false);

        playerName = Login.LoggedInUsername;
        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        filePath = Application.dataPath + "/player_data.txt";

        if (File.Exists(filePath))
        {
            player_data = new List<string>(File.ReadAllLines(filePath));
        }
        else
        {
            player_data = new List<string>();
            File.WriteAllText(filePath, "");
        }
    }

    private void Start()
    {
        SetScore(0);
        SetLives(3);
        SavePelletPositions();
        LoadPlayerData(playerName);
        UpdateHighScoreDisplay(); // Display the high score at the start

        StartCoroutine(StartGameAfterAudio()); // Start the coroutine to begin the game after audio
    }

    public IEnumerator StartGameAfterAudio()
    {
        NewGame(); // Start the game after the audio finishes
        startGameAuido.Play();
        yield return new WaitForSeconds(startGameAuido.clip.length); // Wait for the audio to finish
        ShowButtons(); // Show the buttons after the audio finishes
    }

    private void ShowButtons()
    {
        minimaxButton.gameObject.SetActive(true);
        alphaBetaButton.gameObject.SetActive(true);
        expectimaxButton.gameObject.SetActive(true);
        userButton.gameObject.SetActive(true);
    }

    public void HideButtons()
    {
        minimaxButton.gameObject.SetActive(false);
        alphaBetaButton.gameObject.SetActive(false);
        expectimaxButton.gameObject.SetActive(false);
        userButton.gameObject.SetActive(false);
    }

    private void UpdateHighScoreDisplay()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = PlayerPrefs.GetInt("HighScore_" + sceneName + "_" + playerName, 0).ToString();
        }
    }

    private void Update()
    {
        SetScore(Mathf.Max(0, score)); // Ensure score does not go below 0
        if (lives <= 0 && Input.anyKeyDown && func.buttonName == "")
        {
            lastscore = score;
            StartCoroutine(DelayedRestart(5f)); // Start a delay of 3 seconds before allowing restart
        }
        if (restart)
        {
            lastscore = score;
        }
        CheckHighScore();
        UpdateHighScoreDisplay();
        UpdateUserScoreText();
    }

    private IEnumerator DelayedRestart(float delay)
    {
        yield return new WaitForSeconds(delay);
        score = 0; // Ensure score is reset to 0
        NewGame();

    }

    public void NewGame()
    {
        x=false;
        IsGameOver = true;
        SetScore(0);
        score = 0; // Ensure score is reset to 0
        SetLives(3);
        isPacmanInvincible = false;
        if (restart)
        {
            IsGameOver = false;
            score = 0; // Ensure score is reset to 0

            NewRound();
        }
    }

    private void NewRound()
    {
        pacwoman.transform.rotation = Quaternion.Euler(0, 0, -90); 

        gameOverText.enabled = false;
        gameWinText.enabled = false;

        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
        if (lives > 0)
            func.isPacmanEaten = false;
        ResetState();
    }

    public void CheckHighScore()
    {
        if (score > PlayerPrefs.GetInt("HighScore_" + sceneName + "_" + playerName, 0))
        {
            PlayerPrefs.SetInt("HighScore_" + sceneName + "_" + playerName, score);
        }
    }

    private void ResetState()
    {
        Eaten = false;
        siren.Play();
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].ResetState();
        }
        isPacmanInvincible = false;
        pacwoman.ResetState();
        func.isPacmanEaten = false;
        animationOfPacwoman.Restart();
    }

    public void WinGame()
    {
        gameOverText.text = "You Win!";
        GameOver();
    }

    public void LoseGame()
    {
        gameOverText.text = "Game Over";
        GameOver();
    }

    private void GameOver()
    {
        Eaten = true;
        gameOverr = true;
        restart = false;

        SavePlayerData(playerName, score);
        if(x!=true)
            gameOverText.enabled = true;
        IsGameOver = true;

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(false);
        }

        siren.Stop();
    }

    public void SavePlayerData(string playerName, int score)
    {
        string playerData = playerName + "_" + sceneName + ":" + score;

        bool isPlayerDataExist = false;
        for (int i = 0; i < player_data.Count; i++)
        {
            string data = player_data[i];
            string[] splitData = data.Split(':');
            if (splitData[0] == playerName + "_" + sceneName)
            {
                int existingScore = int.Parse(splitData[1]);
                if (score > existingScore)
                {
                    player_data[i] = playerData;
                }
                isPlayerDataExist = true;
                break;
            }
        }

        if (!isPlayerDataExist)
        {
            player_data.Add(playerData);
        }
        WritePlayerDataToFile();
    }

    private void WritePlayerDataToFile()
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            foreach (string data in player_data)
            {
                writer.WriteLine(data);
            }
        }
    }

    private void LoadPlayerData(string playerName)
    {
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string[] splitData = line.Split(':');
            if (splitData.Length == 2 && splitData[0].Trim() == playerName + "_" + sceneName)
            {
                if (int.TryParse(splitData[1].Trim(), out int userScore))
                {
                    score = userScore;
                    UpdateUserScoreText();
                    return;
                }
            }
        }
        UpdateUserScoreText();
    }

    private void UpdateUserScoreText()
    {
        if (userScoreText != null)
            userScoreText.text = score.ToString();
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
        PlayerPrefs.SetInt("HighScore_" + sceneName + "_" + playerName, Mathf.Max(score, PlayerPrefs.GetInt("HighScore_" + sceneName + "_" + playerName, 0)));
        UpdateHighScoreDisplay(); // Update high score display immediately when score changes
    }

    public void PacmanEaten()
    {
        func.isPacmanEaten = true;
        Eaten = true;
        if (func.buttonName != "User")
            func.pacmanLastMove = Vector2.zero;

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(false);
        }

        SetLives(lives - 1);
        SetScore(Mathf.Max(0, score - 500)); // Ensure score does not go below 0

        if (lives > 0)
        {
            pacwoman.DeathSequence();
            Invoke(nameof(ResetState), 5f);
        }
        else
        {
            pacwoman.DeathSequence();
            deathanimeted.OnDeathAnimationComplete += OnPacwomanDeathSequenceComplete;
        }
        siren.Stop();
        death.Play();
    }

    private void OnPacwomanDeathSequenceComplete()
    {
        pacwoman.gameObject.SetActive(false); 
        GameOver();
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        ghostEatenAudio.Play();
        SetScore(score + points);
        ghost.transform.position = (Vector2)ghostHome.inside.position;
        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        if (currentMunch == 0)
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if (currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }

        pellet.gameObject.SetActive(false);
        SetScore(score + pellet.points);

        pelletPositions.Remove(pellet.transform.position);

        if (!HasRemainingPellets())
        {
            IsGameOver = true;
            pacwoman.gameObject.SetActive(false);
            gameWinText.enabled = true;
            x=true;
            GameOver();
            munch1.Stop();
            munch2.Stop();
            //Invoke(nameof(NewGame), 5f);
        }
    }

    public void SavePelletPositions()
    {
        foreach (Transform pellet in pellets)
        {
            pelletPositions.Add(pellet.position);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].frightened.Enable(pellet.duration);
        }
        isPacmanInvincible = true;
        powerPelletaudio.Play();
        siren.Stop();
        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
        isPacmanInvincible = false;
        powerPelletaudio.Stop();  // Stop playing the power pellet audio
        siren.Play();
    }

    public void FruitEaten(Fruit fruit)
    {
        fruitEatenAudio.Play();
        fruit.gameObject.SetActive(false);
        SetScore(score + fruit.points);
    }

    public bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }
}
