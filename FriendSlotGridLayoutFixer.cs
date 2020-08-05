using UnityEngine;
using UnityEngine.UI;

public class FriendSlotGridLayoutFixer : MonoBehaviour
{
    void OnEnable()
    {
        this.GetComponent<GridLayoutGroup>().cellSize = new Vector2(Screen.currentResolution.width - 50, 0.125f * Screen.currentResolution.height);
    }
}
