using UnityEngine;

public class LandingPad : MonoBehaviour {
    [SerializeField] private int scoreMutiplier;

    public int GetScoreMultiplier() {
        return scoreMutiplier;
    }
}
