using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


public class Functions : MonoBehaviour
{
    public Transform pacman;
    public Transform[] ghostss;
    public GameManager gameManager;
    public List<Vector2> powerPellets;
    public int[,] distances;
    public LayerMask obstacleLayer;
    public int MaxDepth = 5;
    private float totalPellets = 238;
    private float totalPowerPellets = 4;
    public Vector2 currentDirection, pacmanLastMove, lastPosition, lastMoveDirection;
    private float powerPelletFactor, pelletFactor, remainingPellets;
    public string buttonName= "";
    public int multiAgentTurn = 0;
    private PositionToIndexMapper positionToIndexMapper;
    public Pacwoman pacwoman {get; private set; }
    public ChaseDistances chaseDistances { get; private set; }
    public Chrom Chrom { get; private set; }
    public Chrom currentChrom;
    public FruitManager fruitManager { get; private set; }
    public GhostHome ghostHome {get; private set; }
    public Ghost ghost {get; private set; }
    private bool pacmanMoving = false;
    public Vector2 pacmanMove = Vector2.zero;
    public Vector2[] ghostsLastMoves;

    public bool isPacmanEaten=false;
    public bool[] ghostEaten;
    private float decisionTimer = 0f;
    public float decisionInterval = 20f; // Time in seconds to switch behavior
    private bool useAlgorithm = false;

    public float pacmanPunishmentValue;
    public float ghostsPunishmentValue;
    public float ghostsPunishmentValueInv;
    public float pelletsPunishmentValue;
    public float pelletsTooClosePunishmentValue;
    public float powerPelletsPunishmentValue;
    public float powerPelletsTooClosePunishmentValue;
    public float fruitsPunishmentValue;
    public float fruitsTooClosePunishmentValue;


    private void Awake()
    {
        if (ghostss == null || ghostss.Length == 0)
        {
            ghostss = FindObjectsOfType<Transform>().Where(t => t.CompareTag("Ghost")).ToArray();
        }
        ghostHome = FindObjectOfType<GhostHome>();
        ghost = FindObjectOfType<Ghost>();
        positionToIndexMapper = new PositionToIndexMapper(29, 28);
        ghostEaten = new bool[ghostss.Length];
        chaseDistances = FindObjectOfType<ChaseDistances>();
        gameManager = FindObjectOfType<GameManager>();
        fruitManager = FindObjectOfType<FruitManager>();
        pacwoman = FindObjectOfType<Pacwoman>();
        InitializeDistancesMatrix();
        totalPellets = 240;
        totalPowerPellets = 5;
        pacmanLastMove = Vector2.zero;
        ghostsLastMoves = new Vector2[ghostss.Length];

        for (int i = 0; i < ghostss.Length; i++)
        {
            ghostsLastMoves[i] = Vector2.zero;
            ghostEaten[i] = false;
        }
    }

    public void SetCurrentChrom(Chrom Chrom) {
        currentChrom = Chrom;
    }


    private void Update()
    {
        if (gameManager != null)
        {
            HandleVisibility();
            int score = pacwoman.PelletNotEatenPoints();
            gameManager.SetScore(score);

            if (buttonName != "User")
            {
                UpdateRotation();
            }

            UpdatePowerPelletPositions();
            powerPelletFactor = totalPowerPellets / (powerPellets.Count() + 1);
            remainingPellets = gameManager.pelletPositions.Count() + 1;
            pelletFactor = remainingPellets / totalPellets;

            if (buttonName != "")
            {
                decisionTimer += Time.deltaTime;
                if (decisionTimer >= decisionInterval)
                {
                    decisionTimer = 0f;
                    //useAlgorithm = !useAlgorithm; // Toggle behavior
                }
            }

            CheckFrightenedState(); // Frightened durumu kontrol et ve useAlgorithm'i güncelle
        }
        
        CheckGameOver();
    }

    private void CheckFrightenedState()
    {
        foreach (var ghost in ghostss)
        {
            if (ghost.GetComponent<Ghost>().frightened.enabled)
            {
                useAlgorithm = true;
                return;
            }
        }
        useAlgorithm = false; 
    }

    private void CheckGameOver()
    {
        if (remainingPellets == 0)
        {
            gameManager.WinGame();
        }
        else if (gameManager.lives <= 0)
        {
            gameManager.LoseGame();
            buttonName= "";
        }
    }

    private void HandleVisibility()
    {
        for (int i = 0; i < ghostss.Length; i++)
        {
            if ((Vector2)ghostHome.inside.position == (Vector2)ghostss[i].transform.position && !ghostEaten[i])
            {
                ghostEaten[i] = true;
                StartCoroutine(ResetGhostEatenAfterDelay(i, 3f));
            }
        }
    }


    public Vector2 GetRandomMove(Vector2 currentPosition, Vector2 lastMove)
    {
        Vector2 roundedPosition = new Vector2(Mathf.Round(currentPosition.x), Mathf.Round(currentPosition.y));

        // Get possible actions and filter out the reverse of the last move
        List<Vector2> possibleActions = GetPossibleActions(roundedPosition).Where(action => action != -lastMove).ToList();

        // Further filter to ensure the moves are valid
        List<Vector2> validMoves = possibleActions.Where(action => IsValidMove(roundedPosition, action)).ToList();

        if (validMoves.Count == 0)
        {
        
            validMoves = possibleActions;
        }

        if (validMoves.Count == 0)
        {
            
            return Vector2.zero; // No valid moves available, return default value
        }

        return validMoves[Random.Range(0, validMoves.Count)];
    }



    private IEnumerator ResetGhostEatenAfterDelay(int ghostIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        ghostEaten[ghostIndex] = false;
    }

    private void Start()
    {
        currentChrom = new Chrom();
        SetDepthBasedOnScene();
        InvokeRepeating("Move", 0.1f, Random.Range(0.05f, 0.1f));
    }

    private void SetDepthBasedOnScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "PacWoman 1":
                MaxDepth = 3;
                decisionInterval = 9f;
                break;
            case "PW2":
                MaxDepth = 5;
                decisionInterval = 12f;
                break;
            case "PW3":
                MaxDepth = 7;
                decisionInterval = 15f;
                break;
            case "PW4":
                MaxDepth = 9;
                decisionInterval = 18f;
                break;
            default:
                MaxDepth = 5;
                decisionInterval = 10f;
                break;
        }
    }


    public void UpdateRotation()
    {
        if (pacmanMove == new Vector2(1f, 0f))
        {
            pacman.transform.localScale = new Vector3(pacman.transform.localScale.x, Mathf.Abs(pacman.transform.localScale.y), pacman.transform.localScale.z);
        }
        else if (pacmanMove == new Vector2(-1f, 0f))
        {
            pacman.transform.localScale = new Vector3(pacman.transform.localScale.x, -Mathf.Abs(pacman.transform.localScale.y), pacman.transform.localScale.z);
        }

        if (pacmanMove != Vector2.zero)
        {
            float angle = Mathf.Atan2(pacmanMove.y, pacmanMove.x) * Mathf.Rad2Deg;
            pacman.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void Move()
    {
        if (gameManager.lives > 0 && buttonName!= "" && !isPacmanEaten)
        {
            if (multiAgentTurn == 0 && !pacmanMoving)
            {
                switch (buttonName)
                    {
                        case "ButtonMinimax":
                            pacmanMove = MiniMaxDecision(pacman.transform.position, multiAgentTurn, pacmanLastMove);
                            StartCoroutine(MoveCoroutine(pacman, pacman.transform.position, pacmanMove));
                            pacmanLastMove = pacmanMove;
                            break;

                        case "ButtonAlpha_Beta":
                            pacmanMove = AlphaBetaDecision(pacman.transform.position, multiAgentTurn, pacmanLastMove);
                            StartCoroutine(MoveCoroutine(pacman, pacman.transform.position, pacmanMove));
                            pacmanLastMove = pacmanMove;
                            break;

                        case "ButtonExpectimax":
                            pacmanMove = ExpectimaxDecision(pacman.transform.position, multiAgentTurn, pacmanLastMove);
                            StartCoroutine(MoveCoroutine(pacman, pacman.transform.position, pacmanMove));
                            pacmanLastMove = pacmanMove;
                            break;
                        default:
                            break;

                    }
                multiAgentTurn++;
            }
            else if (multiAgentTurn == 1)
            {
                if(!ghostEaten[0]){
                    
                    Vector2 roundedPosition = new Vector2(Mathf.Round(ghostss[0].transform.position.x), Mathf.Round(ghostss[0].transform.position.y));
                    Vector2 ghostMoveMinimax = useAlgorithm ? MiniMaxDecision(roundedPosition, multiAgentTurn, ghostsLastMoves[0]):GetRandomMove(roundedPosition, ghostsLastMoves[0]);
                    StartCoroutine(MoveCoroutine(ghostss[0], roundedPosition, ghostMoveMinimax));
                    ghostsLastMoves[0] = ghostMoveMinimax;

                }
                multiAgentTurn ++;
            }
        
            else if (multiAgentTurn == 2)
            {
                for (int i = 1; i < 3; i++)
                {
                    
                    if(!ghostEaten[i]){
                        Vector2 roundedPosition = new Vector2(Mathf.Round(ghostss[i].transform.position.x), Mathf.Round(ghostss[i].transform.position.y));
                        Vector2 ghostMoveAlpha = useAlgorithm ? AlphaBetaDecision(roundedPosition, multiAgentTurn, ghostsLastMoves[i]): GetRandomMove(roundedPosition, ghostsLastMoves[i]);
                        StartCoroutine(MoveCoroutine(ghostss[i], roundedPosition, ghostMoveAlpha));
                        ghostsLastMoves[i] = ghostMoveAlpha;

                    }
                    multiAgentTurn++;
                }
            }
            else if (multiAgentTurn == 4)
            {
                
                if(!ghostEaten[ghostss.Length - 1]){
                    Vector2 roundedPosition = new Vector2(Mathf.Round(ghostss[ghostss.Length - 1].transform.position.x), Mathf.Round(ghostss[ghostss.Length - 1].transform.position.y));
                    Vector2 ghostMoveExp = useAlgorithm ? ExpectimaxDecision(roundedPosition, multiAgentTurn, ghostsLastMoves[ghostss.Length - 1]):GetRandomMove(roundedPosition, ghostsLastMoves[ghostss.Length - 1]);
                    StartCoroutine(MoveCoroutine(ghostss[ghostss.Length - 1], roundedPosition, ghostMoveExp));
                    ghostsLastMoves[ghostss.Length - 1] = ghostMoveExp;

                }
                multiAgentTurn = 0;
            }
        }
    }

    public IEnumerator MoveCoroutine(Transform characterTransform, Vector2 start, Vector2 direction)
    {
        if (characterTransform == pacman)
        {
            pacmanMoving = true;
        }

        Vector2 end = start + direction.normalized;
        float elapsed = 0;
        float moveDuration = 0.06f;

        while (elapsed < moveDuration)
        {
            characterTransform.position = Vector2.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.position = end;

        // Position correction to integer values
        characterTransform.position = new Vector2(Mathf.Round(characterTransform.position.x), Mathf.Round(characterTransform.position.y));

        if (characterTransform == pacman)
        {
            pacmanMoving = false;
        }
    }

    public Vector2 ExpectimaxDecision(Vector2 currentPosition, int agentIndex, Vector2 lastDir)
{
    float bestValue = float.NegativeInfinity;
    List<Vector2> bestMoves = new List<Vector2>();

    foreach (var action in GetPossibleActions(currentPosition))
    {
        Vector2 newPosition = currentPosition + action;
        float value = ExpectimaxValue(newPosition, 1, agentIndex);

        if (value > bestValue)
        {
            bestValue = value;
            bestMoves.Clear();
            bestMoves.Add(action);
        }
        else if (value == bestValue)
        {
            bestMoves.Add(action);
        }
    }

    if (bestMoves.Count > 1)
    {
        foreach (var move in bestMoves)
        {
            if (move == lastDir)
            {
                return move;
            }
        }
        return bestMoves[Random.Range(0, bestMoves.Count)];
    }

    if (bestMoves.Count == 0)
    {
        
        return Vector2.zero; // Return a default move if no valid moves found
    }

    return bestMoves[0];
}


    public Vector2 AlphaBetaDecision(Vector2 currentPosition, int agentIndex, Vector2 lastDir)
    {   
        float bestValue = float.NegativeInfinity;
        List<Vector2> bestMoves = new List<Vector2>();

        foreach (var action in GetPossibleActions(currentPosition))
        {
            float value = (agentIndex == 0) ?
                MaxValueAlphaBeta(currentPosition + action, 1, float.NegativeInfinity, float.PositiveInfinity, agentIndex) :
                MinValueAlphaBeta(currentPosition + action, 1, float.NegativeInfinity, float.PositiveInfinity, agentIndex);

            if (agentIndex == 0)
            {
                if (value > bestValue)
                {
                    bestValue = value;
                    bestMoves.Clear();
                    bestMoves.Add(action);
                }
                else if (value == bestValue)
                {
                    bestMoves.Add(action);
                }
            }
            else
            {
                if (value > bestValue)
                //if (value > bestValue && action != -currentDirection)
                {
                    bestValue = value;
                    bestMoves.Clear();
                    bestMoves.Add(action);
                }
                else if (value == bestValue)
                //else if (value == bestValue && action != -currentDirection)
                {
                    bestMoves.Add(action);
                }
            }
        }

        if (bestMoves.Count > 1)
        {
            foreach (var move in bestMoves)
            {
                if (move == lastDir)
                {
                    return move;
                }
            }
            return bestMoves[Random.Range(0, bestMoves.Count)];
        }

        return bestMoves[0];
    }


    public Vector2 MiniMaxDecision(Vector2 currentPosition, int agentIndex, Vector2 lastDir)
    {
        
        float bestValue = float.NegativeInfinity;
        List<Vector2> bestMoves = new List<Vector2>();

        foreach (var action in GetPossibleActions(currentPosition))
        {
            float value = (agentIndex == 0) ? MaxValueMiniMax(currentPosition + action, 1, agentIndex) : MinValueMiniMax(currentPosition + action, 1, agentIndex);

            if (value > bestValue)
            {
                bestValue = value;
                bestMoves.Clear();
                bestMoves.Add(action);
            }
            else if (value == bestValue)
            {
                bestMoves.Add(action);
            }
        }

       if (bestMoves.Count > 1)
        {
            foreach (var move in bestMoves)
            {
                if (move == lastDir)
                {
                    return move;
                }
            }

            Vector2 move1 = bestMoves[Random.Range(0, bestMoves.Count)];
            return move1;
        }

        return bestMoves[0];
    }


    private float ExpectimaxValue(Vector2 position, int depth, int agentIndex)
    {
        if (depth >= MaxDepth)
        {
            return Evaluate(position, agentIndex);
        }

        float value = 0;
        //var possibleActions =GetPossibleActions(position);
        float probability = 1f / GetPossibleActions(position).Count;

        foreach (var action in GetPossibleActions(position))
        {
            value += probability * MaxValueExpectimax(position + action, depth + 1, agentIndex);
        }

        return value;
    }

    private float MaxValueExpectimax(Vector2 position, int depth, int agentIndex)
    {
        if (depth >= MaxDepth)
        {
            return Evaluate(position, agentIndex);
        }

        float value = float.NegativeInfinity;
        foreach (var action in GetPossibleActions(position))
        {
            value = Mathf.Max(value, ExpectimaxValue(position + action, depth + 1, agentIndex));
        }

        return value;
    }

    private float MaxValueAlphaBeta(Vector2 position, int depth, float alpha, float beta, int agentIndex)
    {
        if (depth >= MaxDepth)
        {
            return Evaluate(position, agentIndex);
        }

        float value = float.NegativeInfinity;
        foreach (var action in GetPossibleActions(position))
        {
            float tempValue = MinValueAlphaBeta(position + action, depth + 1, alpha, beta, agentIndex);
            value = Mathf.Max(value, tempValue);
            if (value >= beta) return value;
            alpha = Mathf.Max(alpha, value);
        }

        return value;
    }

    private float MinValueAlphaBeta(Vector2 position, int depth, float alpha, float beta, int agentIndex)
    {
        if (depth >= MaxDepth)
        {
            return Evaluate(position, agentIndex);
        }

        float value = float.PositiveInfinity;
        foreach (var action in GetPossibleActions(position))
        {
            currentDirection = action;
            value = Mathf.Min(value, MaxValueAlphaBeta(position + action, depth + 1, alpha, beta, agentIndex));
            if (value <= alpha) return value;
            beta = Mathf.Min(beta, value);
        }

        return value;
    }

    private float MaxValueMiniMax(Vector2 position, int depth, int agentIndex)
    {
        if (depth >= MaxDepth || gameManager.pelletPositions.Count == 1 && position == gameManager.pelletPositions[0])
        {
            return Evaluate(position, agentIndex);
        }

        float value = float.NegativeInfinity;
        foreach (var action in GetPossibleActions(position))
        {
            float nextValue = MinValueMiniMax(position + action, depth + 1, agentIndex);
            value = Mathf.Max(value, nextValue);
        }
        return value;
    }

    private float MinValueMiniMax(Vector2 position, int depth, int agentIndex)
    {
        if (depth >= MaxDepth || gameManager.pelletPositions.Count == 1 && position == gameManager.pelletPositions[0])
        {
            return Evaluate(position, agentIndex);
        }

        float value = float.PositiveInfinity;
        foreach (var action in GetPossibleActions(position))
        {
            float nextValue = MaxValueMiniMax(position + action, depth + 1, agentIndex);
            value = Mathf.Min(value, nextValue);
        }
        return value;
    }


    public float Evaluate(Vector2 position, int agentIndex)
    {
        float score = 0f;
        Ghost[] ghosts = FindObjectsOfType<Ghost>();
        int entityIndex = positionToIndexMapper.GetIndex(position);
        bool isPacman = (agentIndex == 0);
        
        score += EvaluateOtherGhosts(entityIndex, position, ghosts, isPacman); 
        score += EvaluatePellets(position, entityIndex, ghosts, isPacman); 
        score += EvaluatePowerPellets(entityIndex, ghosts, isPacman);
        score += EvaluateFruits(entityIndex, ghosts, isPacman); 

        return score;
    }

    private float EvaluateOtherGhosts(int entityIndex, Vector2 position, Ghost[] ghosts, bool isPacman)
    {
        float score = 0f;
        if(!isPacman){
            Vector2 roundedPacmanPosition = new Vector2(Mathf.Round(pacman.position.x), Mathf.Round(pacman.position.y));
            int pacmanIndex = positionToIndexMapper.GetIndex(roundedPacmanPosition);
            int distanceToPacWoman = distances[pacmanIndex, entityIndex];
            score = (gameManager.isPacmanInvincible ? -10000 : 10000) / Mathf.Max(distanceToPacWoman+1, 1); // -10000 : 10000
        }
            
        for (int i = 0; i < ghosts.Length && i < ghostEaten.Length; i++)
        {
            if (!ghostEaten[i])
            {
                Ghost otherGhost = ghosts[i];
                if (otherGhost != null && otherGhost.gameObject.activeSelf && (Vector2)otherGhost.transform.position != position)
                {
                    Vector2 otherGhostPosition = new Vector2(Mathf.Round(otherGhost.transform.position.x), Mathf.Round(otherGhost.transform.position.y));
                    int otherGhostIndex = positionToIndexMapper.GetIndex(otherGhostPosition);
                    int distance2OtherGhost = distances[entityIndex, otherGhostIndex];

                    if (distance2OtherGhost < 10f)
                    {
                        float ghostScore = (gameManager.isPacmanInvincible ? 5050 : -2050) / Mathf.Max(distance2OtherGhost * distance2OtherGhost, 1); // 5050 : -2050
                        ghostScore = (isPacman ? ghostScore : ghostScore /10);
                        ghostScore = (isPacman && ghost.run ? ghostScore * -1 : ghostScore);
                        score += ghostScore;
                    }
                    if (distance2OtherGhost == 0f && isPacman)
                    {
                        score += -999999;
                    }
                }
            }
        }
        return score;
    }

    private float EvaluatePowerPellets(int pacmanIndex, Ghost[] ghosts, bool isPacman)
    {
        float score = 0f;
        Vector2? closestPowerPellet = null;
        float closestDistance = float.MaxValue;

        foreach (var powerPellet in powerPellets)
        {
            int pelletIndex = positionToIndexMapper.GetIndex(powerPellet);
            int dis2PPellet = distances[pacmanIndex, pelletIndex];

            if (dis2PPellet == 0 && isPacman)
            {
                score += 1000f;
                return score;
            }
            int ghostIndex = positionToIndexMapper.GetIndex(ghost.transform.position);

            bool avoidPellet = ghosts.Any(ghost => distances[ghostIndex, pelletIndex] <= dis2PPellet && !gameManager.isPacmanInvincible);

            if (!avoidPellet && dis2PPellet < closestDistance)
            {
                closestPowerPellet = powerPellet;
                closestDistance = dis2PPellet;
            }
        }

        if (closestPowerPellet.HasValue)
        {
            if (isPacman)
            {
                score += 500f / Mathf.Max(closestDistance + 1, 1); 
            }
        }
        return score;
    }

    private float EvaluatePellets(Vector2 position, int pacmanIndex, Ghost[] ghosts, bool isPacman)
    {
        float score = 0f;

        foreach (var pellet in gameManager.pelletPositions)
        {
            Vector2 pelletPosition = new Vector2(Mathf.Round(pellet.x), Mathf.Round(pellet.y));
            int pelletIndex = positionToIndexMapper.GetIndex(pelletPosition);
            int distanceToPellet = distances[pacmanIndex, pelletIndex];

            if (distanceToPellet == 0 && isPacman)
            {
                score += 1000;
                return score;
            }
            else
            {
                if (isPacman || !gameManager.isPacmanInvincible)
                {
                    score += 100 / Mathf.Max(distanceToPellet + 1, 1);
                }
            }

            if (gameManager.pelletPositions.Count == 1 && position == pelletPosition && isPacman)
            {
                score += 5500;
            }
        }

        return score;
    }

    private float EvaluateFruits(int pacmanIndex, Ghost[] ghosts, bool isPacman)
    {
        float score = 0f;
        foreach (var fruit in fruitManager.fruits)
        {
            if (fruit.activeSelf)
            {
                Vector2 fruitPosition = fruit.transform.position;
                int fruitIndex = positionToIndexMapper.GetIndex(fruitPosition);
                int distanceToFruit = distances[pacmanIndex, fruitIndex];

                if (distanceToFruit == 0 && isPacman)
                {
                    score +=  1200;
                    return score;
                }

                int ghostIndex = positionToIndexMapper.GetIndex(ghost.transform.position);
                bool isGhostTooClose = ghosts.Any(ghost => distances[ghostIndex, fruitIndex] <= 10f);

                if (!isGhostTooClose)
                {
                    if (isPacman || !gameManager.isPacmanInvincible)
                    {
                        score += 900/ Mathf.Max(distanceToFruit + 1, 1);
                    }
                }
            }
        }
        return score;
    }

    public void InitializeDistancesMatrix()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        distances = chaseDistances.InitializeDistancesMatrix(sceneName);
    }

    public void UpdatePowerPelletPositions()
    {
        powerPellets = new List<Vector2>();
        foreach (var pellet in FindObjectsOfType<PowerPellet>())
        {
            powerPellets.Add(pellet.transform.position);
        }
    }

    public class PositionToIndexMapper
    {
        private int mazeWidth;
        private int mazeHeight;
        private Dictionary<Vector2Int, int> positionToIndexMap;

        public PositionToIndexMapper(int mazeWidth, int mazeHeight)
        {
            this.mazeWidth = mazeWidth;
            this.mazeHeight = mazeHeight;
            positionToIndexMap = new Dictionary<Vector2Int, int>();

            int index = 0;
            for (int y = 0; y > -29; y--) //-8   -29
            {
                for (int x = 0; x < 28; x++) //8 28
                {
                    positionToIndexMap[new Vector2Int(x, y)] = index;
                    index++;
                }
            }
        }

        public int GetIndex(Vector2 position)
        {
            Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            if (positionToIndexMap.TryGetValue(posInt, out int index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

        public bool IsValidPosition(Vector2 position)
        {
            Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            return positionToIndexMap.ContainsKey(posInt);
        }

        public Vector2 GetPosition(int index)
        {
            foreach (var pair in positionToIndexMap)
            {
                if (pair.Value == index)
                {
                    return new Vector2(pair.Key.x, pair.Key.y);
                }
            }
            return Vector2.zero;
        }
    }

    public List<Vector2> GetPossibleActions(Vector2 currentPosition)
    {
        List<Vector2> possibleActions = new List<Vector2>();
        Vector2[] allDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 direction in allDirections)
        {
            int currentIndex = positionToIndexMapper.GetIndex(currentPosition);
            int newIndex = positionToIndexMapper.GetIndex(currentPosition + direction);

            if (currentIndex >= 0 && newIndex >= 0 && distances[currentIndex, newIndex] != int.MaxValue)
            {
                // Check if the move is valid
                if (IsValidMove(currentPosition, direction))
                {
                    possibleActions.Add(direction);
                }
            }
        }

        return possibleActions;
    }

    public bool IsValidMove(Vector2 currentPosition, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, 1.0f, obstacleLayer);
        return hit.collider == null;
    }

        // Method to set Pacman punishment value
        public void SetPacmanPunishmentValue(float value)
        {
            pacmanPunishmentValue = value;
            
        }

        // Method to set Ghosts punishment value
        public void SetGhostsPunishmentValue(float value)
        {
            ghostsPunishmentValue = value;
            
        }

        // Method to set Ghosts punishment value (inverse)
        public void SetGhostsPunishmentValueInv(float value)
        {
            ghostsPunishmentValueInv = value;
           
        }

        // Method to set Pellets punishment value
        public void SetPelletsPunishmentValue(float value)
        {
            pelletsPunishmentValue = value;
            
        }

        // Method to set Pellets too close punishment value
        public void SetPelletsTooClosePunishmentValue(float value)
        {
            pelletsTooClosePunishmentValue = value;
            
        }

        // Method to set Power Pellets punishment value
        public void SetPowerPelletsPunishmentValue(float value)
        {
            powerPelletsPunishmentValue = value;
           
        }

        // Method to set Power Pellets too close punishment value
        public void SetPowerPelletsTooClosePunishmentValue(float value)
        {
            powerPelletsTooClosePunishmentValue = value;
            
        }

        // Method to set Fruits punishment value
        public void SetFruitsPunishmentValue(float value)
        {
            fruitsPunishmentValue = value;
            
        }

        // Method to set Fruits too close punishment value
        public void SetFruitsTooClosePunishmentValue(float value)
        {
            fruitsTooClosePunishmentValue = value;
            
        }
}
