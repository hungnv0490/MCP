using UnityEngine;

public class E_MainUI : MonoBehaviour
{
    [SerializeField] private LoadPixcelsPopup loadPixcelsPopup;
    [SerializeField] private E_BallCountPopup ballCountPopup;

    public void LoadPixcelsPopup()
    {
        if (loadPixcelsPopup != null)
            loadPixcelsPopup.Show();
    }

    public void ShowBallCountPopup()
    {
        if (ballCountPopup != null)
            ballCountPopup.Show();
    }
}
