using UnityEngine;


public class EplanationManager : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject startPanel;

    
    public void OpenInfo()
    {
        infoPanel.SetActive(true);
        startPanel.SetActive(false);
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    
}