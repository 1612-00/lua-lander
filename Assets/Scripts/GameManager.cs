using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    private static int levelNumber = 1;
    private static int totalScore = 0;

    public static void ResetStaticData() {
        levelNumber = 1;
        totalScore = 0;
    }


    [SerializeField] private List<GameLevel> gameLevelList;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private int score;
    private float time;
    private bool isTimerActive;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        Lander.Instance.OnCoinPickup += Lander_OnCoinPickup;
        Lander.Instance.OnLanded += Lander_OnLanded;
        Lander.Instance.OnStateChanged += Lander_OnStateChanged;

        GameInput.Instance.OnMenuButtonPressed += GameInput_OnMenuButtonPressed;

        LoadCurrentLevel();
    }

    private void GameInput_OnMenuButtonPressed(object sender, EventArgs e) {
        if (Time.timeScale == 1f) {
            PauseGame();
        }
        else {
            UnPauseGame();
        }
    }

    private void Lander_OnStateChanged(object sender, Lander.OnStateChangedEventArgs e) {
        isTimerActive = e.state == Lander.State.Normal;

        if (e.state == Lander.State.Normal) {
            cinemachineCamera.Target.TrackingTarget = Lander.Instance.transform;
            CinemachineCameraZoom2D.Instance.SetNormalOrthorgraphicSize();
        }
    }

    private void Update() {
        if (isTimerActive) {
            time += Time.deltaTime;
        }
    }

    private void Lander_OnCoinPickup(object sender, System.EventArgs e) {
        AddScore(500);
    }

    private void Lander_OnLanded(object sender, Lander.OnLandedEventArgs e) {
        AddScore(e.score);
    }

    public void AddScore(int amount) {
        score += amount;
    }

    public int GetScore() {
        return score;
    }

    public float GetTime() {
        return time;
    }

    public int GetLevel() {
        return levelNumber;
    }

    private void LoadCurrentLevel() {
        GameLevel gameLevel = GetGameLevel();
        if (gameLevel == null) {
            Debug.LogError($"No GameLevel configured for level {levelNumber}. Check the Game Manager level list.");
            return;
        }

        GameLevel spawnedGameLevel = Instantiate(gameLevel);
        Lander.Instance.transform.position = spawnedGameLevel.GetLanderSpawnPosition();
        cinemachineCamera.Target.TrackingTarget = spawnedGameLevel.GetCameraStartTargetTransform();
        CinemachineCameraZoom2D.Instance.SetTargetOrthorgraphicSize(spawnedGameLevel.GetZoomedOutOrthographicSize());
    }

    private GameLevel GetGameLevel() {
        foreach (GameLevel gameLevel in gameLevelList) {
            if (gameLevel.GetLevelNumber() == levelNumber) {
                return gameLevel;

            }
        }
        return null;
    }

    public int GetTotalScore() {
        return totalScore;
    }

    public static void StartNewGame() {
        levelNumber = 1;
        totalScore = 0;
        Time.timeScale = 1f;
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public void GoToNextLevel() {
        levelNumber++;
        totalScore += score;

        if (GetGameLevel() == null) {
            SceneLoader.LoadScene(SceneLoader.Scene.GameOverScene);
        }
        else {
            SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
        }
    }

    public void RetryLevel() {
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    public void PauseGame() {
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void UnPauseGame() {
        Time.timeScale = 1f;
        OnGameUnpaused?.Invoke(this, EventArgs.Empty);
    }
}
