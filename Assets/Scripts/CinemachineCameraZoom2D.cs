using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraZoom2D : MonoBehaviour {

    private const float NORMAL_ORTHOGRAPHIC_SIZE = 10f;

    public static CinemachineCameraZoom2D Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private float targetOrthorgraphicSize = NORMAL_ORTHOGRAPHIC_SIZE;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update() {
        float zoomSpeed = 2f;
        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(cinemachineCamera.Lens.OrthographicSize, targetOrthorgraphicSize, Time.deltaTime * zoomSpeed);
    }

    public void SetTargetOrthorgraphicSize(float size) {
        this.targetOrthorgraphicSize = size;
    }

    public void SetNormalOrthorgraphicSize() {
        SetTargetOrthorgraphicSize(NORMAL_ORTHOGRAPHIC_SIZE);
    }

}
