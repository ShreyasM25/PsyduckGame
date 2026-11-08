using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartScreenManager : MonoBehaviour
{

    private VisualElement _root;
    private Label _callToAction;
    private Label _highScore;
    
    // Start is called before the first frame update
    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        
        _callToAction = _root.Q<Label>("CallToAction");
        Time.timeScale = 1;
        InvokeRepeating(nameof(BlinkCallToAction), 0.25f, 0.25f);
        
        _highScore = _root.Q<Label>("highScore");
        _highScore.text = "High Score: " + GlobalVariableManager.HighScore.ToString("0");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            SceneManager.LoadScene("Scenes/BaseScene");
        }
    }

    private void BlinkCallToAction()
    {
        _callToAction.visible = !_callToAction.visible;
    }
}
