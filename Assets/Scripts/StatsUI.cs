using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private GameObject speedUpArrowGameObject;
    [SerializeField] private GameObject speedDownArrowGameObject;
    [SerializeField] private GameObject speedLeftArrowGameObject;
    [SerializeField] private GameObject speedRightArrowGameObject;
    [SerializeField] private Image fuelBarImage;

    private void Update() {
        UpdateStatsTextMesh();
    }

    private void UpdateStatsTextMesh() {
        speedUpArrowGameObject.SetActive(Lander.Instance.GetSpeedY() >= 0f);
        speedDownArrowGameObject.SetActive(Lander.Instance.GetSpeedY() < 0f);
        speedLeftArrowGameObject.SetActive(Lander.Instance.GetSpeedX() < 0f);
        speedRightArrowGameObject.SetActive(Lander.Instance.GetSpeedX() >= 0f);

        fuelBarImage.fillAmount = Lander.Instance.GetFuelAmountNomalized();

        statsText.text = GameManager.Instance.GetLevel() + "\n" +
            GameManager.Instance.GetScore() + "\n" +
            Mathf.Abs(Mathf.RoundToInt(GameManager.Instance.GetTime())) + "s\n" +
            Mathf.RoundToInt(Lander.Instance.GetSpeedX()) + "\n" +
            Mathf.RoundToInt(Lander.Instance.GetSpeedY());
    }
}
