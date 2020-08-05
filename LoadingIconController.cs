using UnityEngine;
using UnityEngine.UI;

public class LoadingIconController : MonoBehaviour
{
    public float rotationSpeed;
    public GameObject icon;

    void FixedUpdate()
    {
        if (Globals.showLoadingPanel)
        {
            //this.gameObject.SetActive(true);
            if (icon)
            {
                icon.GetComponent<RectTransform>().eulerAngles = new Vector3(
                icon.GetComponent<RectTransform>().eulerAngles.x,
                icon.GetComponent<RectTransform>().eulerAngles.y,
                icon.GetComponent<RectTransform>().eulerAngles.z - rotationSpeed
                );
            }
        }
        else
        {
            if (icon)
            {
                icon.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
            }
            //this.gameObject.SetActive(false);
        }
    }
}
