using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
// ReSharper disable All

public class GUIManager : MonoBehaviour
{

    private float remainingTime;
    
    private GroupBox _timerGb;
    private Label _time;
    private Label _masterballCounter;

    private GroupBox _reportGb;
    private Label _titleLabel;
    private Label _line1Label;
    private Label _line2Label;
    private Button _returnButton;

    private LevelManager _levelmanager;

    private bool playedMusic = false;
    private bool playedPing = false;
    [SerializeField] private AudioClip intenseMusic;
    [SerializeField] private AudioClip timePing;
    private AudioSource _cameraAs1;
    private AudioSource _cameraAs2;

    [SerializeField] private GameObject trainer;
    private int masterballCount;
    
    [SerializeField] private AudioClip buttonPress;
    private StyleColor originalButtonColor;
    
    // Start is called before the first frame update
    void Start()
    {
        _levelmanager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        _timerGb = root.Q<GroupBox>("timer");
        _time = root.Q<Label>("time");
        _masterballCounter = root.Q<Label>("masterballCounter");
        
        _reportGb = root.Q<GroupBox>("report");
        _titleLabel = root.Q<Label>("title");
        _line1Label = root.Q<Label>("line1");
        _line2Label = root.Q<Label>("line2");
        _returnButton = root.Q<Button>("return");

        _returnButton.clicked += ReturnButtonPressed;
        _returnButton.RegisterCallback<MouseEnterEvent>(OnButtonHover);
        _returnButton.RegisterCallback<MouseLeaveEvent>(OnButtonLeave);
        
        SetDisplay(_reportGb, false);
        SetDisplay(_timerGb, true);
        
        _cameraAs1 = GameObject.Find("Main Camera").GetComponents<AudioSource>()[0];
        _cameraAs2 = GameObject.Find("Main Camera").GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        masterballCount = trainer.GetComponent<PlayerController>().masterballCount;
        
        remainingTime = _levelmanager.endTime - _levelmanager.currentTime;
        
        if (GetDisplay())
        {
            _time.text = remainingTime.ToString("0");
            _masterballCounter.text = masterballCount.ToString("0") + " Master Balls remaining";
        }

        if (remainingTime < 10 && !playedMusic)
        {
            _time.style.color = Color.red;
            _time.style.fontSize = 175;
            playMusic();
            playedMusic = true;
        }
        
        if (remainingTime < 5 && !playedPing)
        {
            playPing();
            playedPing = true;
        }

        if (remainingTime <= 0)
        {
            stopMusic();
        }
    }

    public void ReportToPlayer(string title, string line1, float time)
    {
        if (GetDisplay())
        {
            CancelInvoke(nameof(HideReport));
        }
        SetDisplay(_timerGb, false);
        SetDisplay(_reportGb, true);
        _titleLabel.text = title;
        _line1Label.text = line1;
        _titleLabel.visible = true;
        _line1Label.visible = true;
        _line2Label.visible = false;
        _returnButton.visible = false;
        Invoke(nameof(HideReport), time);
    }
    
    public void ReportToPlayer(string title, string line1, string line2)
    {
        if (GetDisplay())
        {
            CancelInvoke(nameof(HideReport));
        }
        SetDisplay(_timerGb, false);
        SetDisplay(_reportGb, true);
        _titleLabel.text = title;
        _line1Label.text = line1;
        _line2Label.text = line2;
        _titleLabel.visible = true;
        _line1Label.visible = true;
        _line2Label.visible = true;
        _returnButton.visible = true;
        _returnButton.text = "Return to Start";
    }

    private void HideReport()
    {
        SetDisplay(_timerGb, true);
        SetDisplay(_reportGb, false);
    }

    private void ReturnButtonPressed()
    {
        _cameraAs2.clip = buttonPress;
        _cameraAs2.Play();
        SceneManager.LoadScene("Scenes/Start");
    }
    
    private void OnButtonHover(MouseEnterEvent buttonEvent)
    {
        originalButtonColor = _returnButton.resolvedStyle.backgroundColor;
        _returnButton.style.backgroundColor = Color.red;
    }

    private void OnButtonLeave(MouseLeaveEvent buttonEvent)
    {
        _returnButton.style.backgroundColor = originalButtonColor;
    }

    private void SetDisplay(GroupBox gb, bool display)
    {
        gb.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private bool GetDisplay()
    {
        if (_timerGb.style.display == DisplayStyle.Flex)
        {
            return true;
        }
        return false;
    }

    private void playMusic()
    {
        _cameraAs1.clip = intenseMusic;
        _cameraAs1.Play();
    }

    private void playPing()
    {
        _cameraAs2.clip = timePing;
        _cameraAs2.Play();
    }

    private void stopMusic()
    {
        _cameraAs1.Stop();
    }
}
