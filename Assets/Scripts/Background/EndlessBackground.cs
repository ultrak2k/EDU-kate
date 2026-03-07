using UnityEngine;
using UnityEngine.UIElements;


public class EndlessBackground : MonoBehaviour
{
    [Header("References")]
    [Tooltip("3 background, ordered left to right.")]
    public PlayerController PlayerCont;

    public Transform[] backgrounds = new Transform[3];
    public GameObject[] objectsToRandomize; // for random scaling of objects etc.

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

    
    public GameObject Level1;

    // Tracks which panel is currently the leftmost, middle, rightmost
    private int leftIndex = 0;
    private int middleIndex = 1;
    private int rightIndex = 2;

    //for random scaling of objects etc.
    private int _lengthsTravelled = 0;

    public int SpawnLevelRight = 2;

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

        // Moving RIGHT: left panel  too far behind
        float leftPanelX = backgrounds[leftIndex].position.x;
        if (playerX - leftPanelX > recycleDistance)
        {

            // SpawnLevel(bool Right) is confusing, but right is true 
            SpawnLevelRight = 1;
            SpawnLevel(SpawnLevelRight);
        }

        // Moving LEFT: right panel too far behind
        float rightPanelX = backgrounds[rightIndex].position.x;
        if (rightPanelX - playerX > recycleDistance)
        {
            SpawnLevelRight = 2;
            SpawnLevel(SpawnLevelRight); // left is false
        }
    }
    private void SnapPanelsToStartPositions()
    {
        // Use panel 0 as anchor
        float baseX = backgrounds[0].position.x;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector3 pos = backgrounds[i].position;
            pos.x = baseX + backgroundWidth * i;
            backgrounds[i].position = pos;
        }
    }
    public void SpawnLevel(int SpawnLevelInt) //confusing using a bool but im sorry
    {
        if (PlayerCont.DialoguesTriggered == 1)
        {
            SpawnLevelInt = 0;
            PlayerCont.DialoguesTriggered = 0;
        }
        //Moving RIGHT spawn level:
        if (SpawnLevelInt == 0)
        {
            Transform OldEndless = backgrounds[leftIndex];

            float newX = backgrounds[rightIndex].position.x + backgroundWidth;
            Vector3 pos = backgrounds[leftIndex].position;
            pos.x = newX;

            

            if (PlayerCont.DialoguesTriggered == 1)
            {
                backgrounds[leftIndex] = Level1.transform;
                backgrounds[leftIndex].position = pos;
            }
            else
            {
                backgrounds[leftIndex].position = pos;
            }

            backgrounds[leftIndex] = OldEndless;

            int oldLeft = leftIndex;
            leftIndex = middleIndex;
            middleIndex = rightIndex;
            rightIndex = oldLeft;

            return;
        }
        //Moving LEFT:
        else if(SpawnLevelInt == 1)
        {
            float newX = backgrounds[leftIndex].position.x - backgroundWidth;
            Vector3 pos = backgrounds[rightIndex].position;
            pos.x = newX;
            
            backgrounds[rightIndex].position = pos;
         
            int oldRight = rightIndex;
            rightIndex = middleIndex;
            middleIndex = leftIndex;
            leftIndex = oldRight;
        }
        //Moving Right:
        else if (SpawnLevelInt == 2)
        {
            float newX = backgrounds[rightIndex].position.x - backgroundWidth;
            Vector3 pos = backgrounds[leftIndex].position;
            pos.x = newX;

            backgrounds[leftIndex].position = pos;

            int oldLeft = leftIndex;
            leftIndex = middleIndex;
            middleIndex = rightIndex;
            rightIndex = oldLeft;
        }

    }
    private bool DebugNonsense()
    {
        if (backgrounds == null || backgrounds.Length != 3)
        {
            Debug.LogError("[EndlessBackground] You must assign exactly 3 backgrounds inda Inspector.");
            return false;
        }
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null)
            {
                Debug.LogError($"[EndlessBackground] backgrounds[{i}] not assigned.");
                return false;
            }
        }
        if (player == null)
        {
            Debug.LogError("[EndlessBackground] Player Transform not assigned.");
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