using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainManager : MonoBehaviour
{
  public Brick BrickPrefab;
  public int LineCount = 6;
  public GameObject Ball;
  private Rigidbody BallRigidbody;
  public Text ScoreText;
  public Text BestScoreText;
  public GameObject GameOverText;
  public GameObject MenuCanvas;
  public TMP_InputField NameInputField;
  public Button StartButton;
  private int m_Points;
  private bool m_GameOver = false;
  public static MainManager Instance;
  private string currentPlayerName = "No Name";
  public List<KeyValuePair<string, int>> player = new List<KeyValuePair<string, int>>();
  public List<GameObject> activeBricks = new List<GameObject>();
  public bool bricksNeedRespawn = false;
  private void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
    LoadPlayers();
    
    //PlayerPrefs.DeleteAll();
    //PlayerPrefs.Save(); //To clear scores
    //Debug.Log("Test data cleared.");
  }
  void Start()
  {
    const float step = 0.6f;
    int perLine = Mathf.FloorToInt(4.0f / step);

    int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
    for (int i = 0; i < LineCount; ++i)
    {
      for (int x = 0; x < perLine; ++x)
      {
        Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
        var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
        brick.PointValue = pointCountArray[i];
        brick.onDestroyed.AddListener(AddPoint);

        // Add the brick to the active list
        activeBricks.Add(brick.gameObject);
      }
    }

    UpdateBestScoreText();
    MenuCanvas.SetActive(true);
    GameOverText.SetActive(false);
    BallRigidbody = Ball.GetComponent<Rigidbody>();
    StartButton.onClick.AddListener(StartGame);
  }
  public void SetBricksNeedRespawn(bool needRespawn)
  {
    bricksNeedRespawn = needRespawn;
  }
  private void Update()
  {
    if (m_GameOver && Input.GetKeyDown(KeyCode.Space))
    {
      ResetGame();
      return;
    }
    if (StartButton.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space))
    {
      StartGame();
    }
  }
  public void StartGame()
  {
    currentPlayerName = NameInputField.text;

    if (string.IsNullOrWhiteSpace(currentPlayerName))
    {
      currentPlayerName = "Player";
    }

    NameInputField.gameObject.SetActive(false);
    StartButton.gameObject.SetActive(false);

    // If the Ball reference is null, find the existing ball in the scene
    if (Ball == null || !BallRigidbody)
    {
      Ball = GameObject.FindGameObjectWithTag("Ball");
      if (Ball != null)
      {
        BallRigidbody = Ball.GetComponent<Rigidbody>();
      }
      else
      {
        Debug.LogError("Ball object is missing from the scene!");
        return;
      }
    }

    // Reset ball properties
    Ball.transform.position = new Vector3(0, 1, 0); // Reset position
    Ball.transform.SetParent(null);                // Detach from parent (if any)

    // Re-enable physics
    BallRigidbody.isKinematic = false;
    BallRigidbody.velocity = Vector3.zero;         // Reset velocity

    // Launch the ball
    float randomDirection = Random.Range(-1.0f, 1.0f);
    Vector3 forceDir = new Vector3(randomDirection, 1, 0);
    forceDir.Normalize();
    BallRigidbody.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
  }

  void AddPoint(int point)
  {
    m_Points += point;
    ScoreText.text = $"Score: {m_Points}";
  }

  public void GameOver()
  {
    m_GameOver = true;
    GameOverText.SetActive(true);

    if (Ball != null)
    {
      // Reset the ball's position and velocity
      Ball.transform.position = new Vector3(0, 1, 0);
      BallRigidbody.velocity = Vector3.zero;

      // Disable physics to stop sinking or unintended movement
      BallRigidbody.isKinematic = true;

      // Optionally, attach the ball to the MenuCanvas (inactive state)
      Ball.transform.SetParent(MenuCanvas.transform);
    }

    // Ensure bricks are ready for the next game
    RespawnBricks();

    // Update best score logic
    if (player.Count > 0)
    {
      var bestPlayer = player[0];
      if (m_Points > bestPlayer.Value) // If current score is higher
      {
        //Debug.Log($"New best score! {currentPlayerName}: {m_Points}");
        player[0] = new KeyValuePair<string, int>(currentPlayerName, m_Points); // Update best score
      }
    }
    else
    {
      // If no players exist, add the current player as the best score
      player.Add(new KeyValuePair<string, int>(currentPlayerName, m_Points));
    }

    SavePlayers(); // Save updated player data
    UpdateBestScoreText(); // Refresh UI to show new best score
  }

  void ResetGame()
  {
    foreach (var brick in activeBricks)
    {
      if (brick != null)
      {
        Destroy(brick);
      }
    }
    activeBricks.Clear();

    // Respawn bricks
    const float step = 0.6f;
    int perLine = Mathf.FloorToInt(4.0f / step);

    int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
    for (int i = 0; i < LineCount; ++i)
    {
      for (int x = 0; x < perLine; ++x)
      {
        Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
        var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
        brick.PointValue = pointCountArray[i];
        brick.onDestroyed.AddListener(AddPoint);

        // Add the new brick to the active list
        activeBricks.Add(brick.gameObject);
      }
    }

    // Reset game state
    m_Points = 0;
    m_GameOver = false;
    StartButton.gameObject.SetActive(true);
    NameInputField.gameObject.SetActive(true);
    GameOverText.SetActive(false);
    ScoreText.text = "Score: 0";
  }
  public bool CheckBricksCleared()
  {
    if (activeBricks.Count == 0)
    {
      bricksNeedRespawn = true; // Set the flag for the paddle to respawn bricks
      return true;
    }
    return false;
  }
  public void RespawnBricks()
  {
    // Clear the bricks
    foreach (var brick in activeBricks)
    {
      if (brick != null)
      {
        Destroy(brick);
      }
    }
    activeBricks.Clear();

    // Create new bricks
    const float step = 0.6f;
    int perLine = Mathf.FloorToInt(4.0f / step);

    int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
    for (int i = 0; i < LineCount; ++i)
    {
      for (int x = 0; x < perLine; ++x)
      {
        Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
        var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
        brick.PointValue = pointCountArray[i];
        brick.onDestroyed.AddListener(AddPoint);

        // Add the new brick to the active list
        activeBricks.Add(brick.gameObject);
      }
    }
    bricksNeedRespawn = false;
  }
  void UpdateBestScoreText()
  {
    if (player.Count > 0)
    {
      var bestPlayer = player[0];
      BestScoreText.text = $"Best Score: {bestPlayer.Key} : {bestPlayer.Value}";
    }
    else
    {
      BestScoreText.text = "Best Score: No Name : 0";
    }
  }

  void SavePlayers()
  {
    PlayerPrefs.SetInt("PlayerCount", player.Count);

    for (int i = 0; i < player.Count; i++)
    {
      PlayerPrefs.SetString($"PlayerName{i}", player[i].Key);
      PlayerPrefs.SetInt($"PlayerScore{i}", player[i].Value);
    }

    PlayerPrefs.Save();
  }

  void LoadPlayers()
  {
    player.Clear();

    int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
    for (int i = 0; i < playerCount; i++)
    {
      string name = PlayerPrefs.GetString($"PlayerName{i}", "No Name");
      int score = PlayerPrefs.GetInt($"PlayerScore{i}", 0);
      player.Add(new KeyValuePair<string, int>(name, score));
    }
  }
}