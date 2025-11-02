using UnityEngine;

public class UIColorPalette : MonoBehaviour
{
    public static UIColorPalette Instance { get; private set; }
    
    [Header("Main Colors")]
    public Color PrimaryColor;
    public Color DarkBackground;
    public Color AccentColor;
    public Color SurfaceColor;
    public Color AlertColor;

    [Header("Text Colors")]
    public Color TextPrimary;
    public Color TextSecondary;
    public Color TextDisabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
