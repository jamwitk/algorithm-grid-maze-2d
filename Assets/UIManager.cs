using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
// spce

    [Header("Main Menu UI")]
    public TMP_Text selectedAlgorithm;
    public void OnSolveButtonClick()
    {
        GameController.instance.SolvePuzzle();
    }
    
    public void OnAlgorithmButtonClick(int index)
    {
        SetAlgorithm(index);
    }
    public void SetAlgorithm(int index)
    {
        selectedAlgorithm.text = ((Algorithm)index).ToString();
        GameController.instance.SetAlgorithm(index);
    }
}
