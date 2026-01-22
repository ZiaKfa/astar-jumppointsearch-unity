using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public GameController gameController;
    public Image hpBarImage;

    private void Update()
    {
        if (gameController != null && hpBarImage != null)
        {
            hpBarImage.transform.localScale = new Vector3((float)gameController.GetHP() / (float)gameController.GetMaxHP(),hpBarImage.transform.localScale.y,  hpBarImage.transform.localScale.z);
        }
    }
}