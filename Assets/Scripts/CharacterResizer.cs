using UnityEngine;

public class CharacterResizer : MonoBehaviour
{
    [Header("Scales")]
    public Vector3 smallScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 normalScale = Vector3.one;
    public Vector3 largeScale = new Vector3(2f, 2f, 2f);

    [Header("Models")]
    public GameObject smallModel;
    public GameObject normalModel;
    public GameObject largeModel;

    [Header("Resize")]
    public float resizeSpeed = 5f;

    private PlayerController playerController;
    private Vector3 targetScale;
    private bool isResizing;

    public enum SizeState { Small, Normal, Large }
    public SizeState currentState { get; private set; }

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("❌ No PlayerController");
            enabled = false;
            return;
        }

        // 🔥 SUSCRIPCIÓN A EVENTOS
        playerController.OnResizeSmall += () => SetState(SizeState.Small);
        playerController.OnResizeNormal += () => SetState(SizeState.Normal);
        playerController.OnResizeLarge += () => SetState(SizeState.Large);

        targetScale = normalScale;
        currentState = SizeState.Normal;

        transform.localScale = normalScale;

        UpdateModel();
    }

    void Update()
    {
        HandleResize();
    }

    void HandleResize()
    {
        if (!isResizing) return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * resizeSpeed
        );

        if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
        {
            transform.localScale = targetScale;
            isResizing = false;

            UpdateModel();
        }
    }

    void SetState(SizeState newState)
    {
        if (newState == currentState) return;

        currentState = newState;

        switch (newState)
        {
            case SizeState.Small:
                targetScale = smallScale;
                playerController.SetMovementStats(10f, 8f, true);
                break;

            case SizeState.Normal:
                targetScale = normalScale;
                playerController.SetMovementStats(8f, 12f, true);
                break;

            case SizeState.Large:
                targetScale = largeScale;
                playerController.SetMovementStats(5f, 0f, false);
                break;
        }

        isResizing = true;
    }

    void UpdateModel()
    {
        if (smallModel != null) smallModel.SetActive(false);
        if (normalModel != null) normalModel.SetActive(false);
        if (largeModel != null) largeModel.SetActive(false);

        switch (currentState)
        {
            case SizeState.Small:
                if (smallModel != null) smallModel.SetActive(true);
                break;

            case SizeState.Normal:
                if (normalModel != null) normalModel.SetActive(true);
                break;

            case SizeState.Large:
                if (largeModel != null) largeModel.SetActive(true);
                break;
        }
    }
}