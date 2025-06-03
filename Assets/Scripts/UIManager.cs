using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
// spce
    public static UIManager instance;
    

    [Header("Main Menu UI")]
    public TMP_Text selectedAlgorithm;
    [Header("Algorithm Buttons")]
    public GameObject geneticAlgorithmButton;
    public GameObject astarAlgorithmButton;
    public GameObject dijskraAlgorithmButton;
    public TMP_Text startTileText;
    
    public void Awake(){
        instance = this;
    }
    public void OnSolveButtonClick()
    {
        GameController.instance.SolvePuzzle();
    }
    
    public void OnAlgorithmButtonClick(int index)
    {
        SetAlgorithm(index);
        SetButton(index, true);
    }
    public void OnResetButtonClick(){
        SetButton(0, false);
        SetButton(1, false);
        SetButton(2, false);
        selectedAlgorithm.text = "Select Algorithm";
        startTileText.text = "Start: 0, 0\nEnd: 0, 0";
        GameController.instance.Reset();
    }
    public void SetAlgorithm(int index)
    {
        selectedAlgorithm.text = "Selected Algorithm: "+((Algorithm)index).ToString();
        GameController.instance.SetAlgorithm(index);
    }
    public void SetStartEndTile(Vector3Int start, Vector3Int end){
        startTileText.text = "Start: " + start.x + ", " + start.y + "\n" + "End: " + end.x + ", " + end.y;
    }
    public void SetButton(int index, bool active){
        if(index == 0){
            geneticAlgorithmButton.GetComponent<Image>().color = active ? Color.green : Color.white;
            astarAlgorithmButton.GetComponent<Image>().color = Color.white;
            dijskraAlgorithmButton.GetComponent<Image>().color = Color.white;
        }
        else if(index == 1){
            astarAlgorithmButton.GetComponent<Image>().color = active ? Color.green : Color.white;
            geneticAlgorithmButton.GetComponent<Image>().color = Color.white;
            dijskraAlgorithmButton.GetComponent<Image>().color = Color.white;
        }
        else if(index == 2){
            dijskraAlgorithmButton.GetComponent<Image>().color = active ? Color.green : Color.white;
            geneticAlgorithmButton.GetComponent<Image>().color = Color.white;
            astarAlgorithmButton.GetComponent<Image>().color = Color.white;
        }
    }
}
