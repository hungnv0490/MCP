using UnityEngine;

public class E_MainUI : MonoBehaviour
{
    [SerializeField] private LoadPixcelsPopup loadPixcelsPopup;

    public void LoadPixcelsPopup()
    {
        if (loadPixcelsPopup != null)
            loadPixcelsPopup.Show();
    }
}
