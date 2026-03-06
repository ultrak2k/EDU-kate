using UnityEngine;
using UnityEngine.UIElements;


public class EndlessBackground : MonoBehaviour
{
    [Header("References")]
    [Tooltip("3 background, ordered left to right.")]
    public Transform[] backgrounds = new Transform[3];

    [Tooltip("The player.")]
    public Transform player;

    [Header("Settings")]
    [Tooltip("The world-space width of a single background panel.")]
    public float backgroundWidth = 20f;

    [Tooltip("1 = player moves right, -1 = player moves left.")]
    public float scrollDirection = 1f;

    [Tooltip("How far behind the player a panel must be before it teleports to the front. idk play with this")]
    [Range(0f, 2f)]
    public float recycleThresholdMultiplier = 1.5f;

    

    // Tracks which panel is currently the leftmost, middle, rightmost
    private int leftIndex = 0;
    private int middleIndex = 1;
    private int rightIndex = 2;

    private void Start()
    {
        if (!DebugNonsense()) return;
        SnapPanelsToStartPositions();
    }

    private void LateUpdate()
    {
        if (player == null || backgrounds == null || backgrounds.Length != 3) return;

        float playerX = player.position.x;
        float recycleDistance = backgroundWidth * recycleThresholdMultiplier;

        if (scrollDirection >= 0)
        {
            // Moving RIGHT: recycle left panel to the right
            float leftPanelX = backgrounds[leftIndex].position.x;

            if (playerX - leftPanelX > recycleDistance)
            {
                // Teleport the leftmost panel to the right of the rightmost panel
                float newX = backgrounds[rightIndex].position.x + backgroundWidth;
                Vector3 pos = backgrounds[leftIndex].position;
                pos.x = newX;
                backgrounds[leftIndex].position = pos;

                // Rotate the index tracking
                int oldLeft = leftIndex;
                leftIndex = middleIndex;
                middleIndex = rightIndex;
                rightIndex = oldLeft;
            }
        }
        else
        {
            // Moving LEFT: recycle right panel to the left // this doesnt work?
            float rightPanelX = backgrounds[rightIndex].position.x;

            if (rightPanelX - playerX > recycleDistance)
            {
                // Teleport the rightmost panel to the left of the leftmost panel
                float newX = backgrounds[leftIndex].position.x - backgroundWidth;
                Vector3 pos = backgrounds[rightIndex].position;
                pos.x = newX;
                backgrounds[rightIndex].position = pos;

                // Rotate the index tracking
                int oldRight = rightIndex;
                rightIndex = middleIndex;
                middleIndex = leftIndex;
                leftIndex = oldRight;
            }
        }
    }
    private void SnapPanelsToStartPositions()
    {
        // Use panel 0 as the anchor
        float baseX = backgrounds[0].position.x;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector3 pos = backgrounds[i].position;
            pos.x = baseX + backgroundWidth * i;
            backgrounds[i].position = pos;
        }
    }

    private bool DebugNonsense()
    {
        if (backgrounds == null || backgrounds.Length != 3)
        {
            Debug.LogError("[EndlessBackground] You must assign exactly 3 backgrounds in the Inspector.");
            return false;
        }
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null)
            {
                Debug.LogError($"[EndlessBackground] backgrounds[{i}] is not assigned.");
                return false;
            }
        }
        if (player == null)
        {
            Debug.LogError("[EndlessBackground] Player Transform is not assigned.");
            return false;
        }
        if (backgroundWidth <= 0f)
        {
            Debug.LogError("[EndlessBackground] backgroundWidth must be greater than 0.");
            return false;
        }
        return true;
    }

    //panel boundaries visual
    private void OnDrawGizmos()
    {
        if (backgrounds == null) return;

        Gizmos.color = Color.cyan;
        foreach (var bg in backgrounds)
        {
            if (bg == null) continue;
            Vector3 center = bg.position;
            Gizmos.DrawWireCube(center, new Vector3(backgroundWidth, 10f, 0f));
        }
    }
}