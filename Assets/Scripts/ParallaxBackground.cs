using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Parallax background effect for UI Canvas images.
/// Designed for Screen Space - Camera canvas mode with multiple layers.
/// Each layer moves at a different speed based on the camera position.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("The RectTransform/Image component for this layer")]
        public RectTransform layerTransform;
        
        [Tooltip("Parallax speed multiplier (0 = no movement, 1 = moves with camera)")]
        [Range(0f, 1f)]
        public float parallaxSpeed = 0.5f;
        
        [Tooltip("Enable horizontal parallax")]
        public bool enableHorizontal = true;
        
        [Tooltip("Enable vertical parallax")]
        public bool enableVertical = true;
        
        [HideInInspector]
        public Vector2 startPosition;
    }

    [Header("Parallax Settings")]
    [SerializeField] private ParallaxLayer[] layers = new ParallaxLayer[3];
    
    [Header("Camera Reference")]
    [SerializeField] private Camera parallaxCamera;
    
    [Header("Movement Settings")]
    [SerializeField] private float parallaxMultiplier = 1f;
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothSpeed = 5f;

    private Vector3 previousCameraPosition;
    private Canvas parentCanvas;

    private void Start()
    {
        // Find the parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (parentCanvas == null)
        {
            Debug.LogError("ParallaxBackground: No Canvas found in parent hierarchy!");
            enabled = false;
            return;
        }

        // Auto-assign camera if not set
        if (parallaxCamera == null)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                parallaxCamera = parentCanvas.worldCamera;
            }
            
            if (parallaxCamera == null)
            {
                parallaxCamera = Camera.main;
            }
            
            if (parallaxCamera == null)
            {
                Debug.LogError("ParallaxBackground: No camera assigned or found!");
                enabled = false;
                return;
            }
        }

        // Initialize layers
        InitializeLayers();
        previousCameraPosition = parallaxCamera.transform.position;
    }

    private void InitializeLayers()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerTransform != null)
            {
                layers[i].startPosition = layers[i].layerTransform.anchoredPosition;
            }
            else
            {
                Debug.LogWarning($"ParallaxBackground: Layer {i} has no RectTransform assigned!");
            }
        }
    }

    private void LateUpdate()
    {
        if (parallaxCamera == null) return;

        // Calculate camera movement delta
        Vector3 cameraMovement = parallaxCamera.transform.position - previousCameraPosition;
        
        // Update each layer
        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform == null) continue;

            // Calculate parallax offset
            Vector2 parallaxOffset = Vector2.zero;
            
            if (layer.enableHorizontal)
            {
                parallaxOffset.x = -cameraMovement.x * layer.parallaxSpeed * parallaxMultiplier;
            }
            
            if (layer.enableVertical)
            {
                parallaxOffset.y = -cameraMovement.y * layer.parallaxSpeed * parallaxMultiplier;
            }

            // Apply movement
            Vector2 targetPosition = layer.layerTransform.anchoredPosition + parallaxOffset;
            
            if (smoothMovement)
            {
                layer.layerTransform.anchoredPosition = Vector2.Lerp(
                    layer.layerTransform.anchoredPosition,
                    targetPosition,
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                layer.layerTransform.anchoredPosition = targetPosition;
            }
        }

        // Store current camera position for next frame
        previousCameraPosition = parallaxCamera.transform.position;
    }

    /// <summary>
    /// Reset all layers to their starting positions
    /// </summary>
    public void ResetLayers()
    {
        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                layer.layerTransform.anchoredPosition = layer.startPosition;
            }
        }
        
        if (parallaxCamera != null)
        {
            previousCameraPosition = parallaxCamera.transform.position;
        }
    }

    /// <summary>
    /// Set the parallax speed for a specific layer
    /// </summary>
    public void SetLayerSpeed(int layerIndex, float speed)
    {
        if (layerIndex >= 0 && layerIndex < layers.Length)
        {
            layers[layerIndex].parallaxSpeed = Mathf.Clamp01(speed);
        }
    }

    /// <summary>
    /// Get the number of configured layers
    /// </summary>
    public int GetLayerCount()
    {
        return layers.Length;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure we have exactly 3 layers
        if (layers == null || layers.Length != 3)
        {
            ParallaxLayer[] newLayers = new ParallaxLayer[3];
            
            if (layers != null)
            {
                for (int i = 0; i < Mathf.Min(layers.Length, 3); i++)
                {
                    newLayers[i] = layers[i];
                }
            }
            
            // Initialize any null layers
            for (int i = 0; i < 3; i++)
            {
                if (newLayers[i] == null)
                {
                    newLayers[i] = new ParallaxLayer
                    {
                        parallaxSpeed = (i + 1) * 0.25f, // 0.25, 0.5, 0.75
                        enableHorizontal = true,
                        enableVertical = true
                    };
                }
            }
            
            layers = newLayers;
        }
    }
#endif
}
