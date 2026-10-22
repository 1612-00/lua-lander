using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    private static int levelNumber = 1;
    [SerializeField] private List<GameLevel> gameLevelList;

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

        LoadCurrentLevel();
    }

    private void Lander_OnStateChanged(object sender, Lander.OnStateChangedEventArgs e) {
        isTimerActive = e.state == Lander.State.Normal;
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
        foreach (GameLevel gameLevel in gameLevelList) {
            if (gameLevel.GetLevelNumber() == levelNumber) {
                GameLevel spawnedGameLevel = Instantiate(gameLevel);
                Lander.Instance.transform.position = spawnedGameLevel.GetLanderSpawnPosition();
            }
        }
    }

    public void GoToNextLevel() {
        levelNumber++;
        SceneManager.LoadScene(0);
    }

    public void RetryLevel() {
        SceneManager.LoadScene(0);
    }
}
