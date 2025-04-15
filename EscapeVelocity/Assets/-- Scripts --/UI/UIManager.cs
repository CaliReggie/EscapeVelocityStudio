using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

[RequireComponent(typeof(Canvas))]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [field: SerializeField] public TextMeshProUGUI SpeedText { get; private set; }
    
    [field: SerializeField] public TextMeshProUGUI YVelText { get; private set; }
    
    [field: SerializeField] public TextMeshProUGUI MoveStateText { get; private set; }
    
    [field: SerializeField] public TextMeshProUGUI WallStateText { get; private set; }
    
    [field: SerializeField] public TextMeshProUGUI PredictionStateText { get; private set; }

    [Header("Child References")]
    
    [SerializeField] private GameObject mainMenuPage;
    
    [SerializeField] private GameObject playerHUDPage;
    
    [SerializeField] private UISelectWheel uiSelectWheel;
    
    [SerializeField] private GameObject pausePage;
    
    [SerializeField] private GameObject gameOverPage;
    
    [SerializeField] private TextMeshProUGUI timeText;
        
    [Header("Holders")]
    
    [SerializeField] private Transform iconHolder;
    
    // [Header("Interactable Display Settings")]
    //
    // [SerializeField]
    // private GameObject interactableDisplay;
    //
    // [SerializeField]
    // private EScreenPos interactableDisplayPos = EScreenPos.BottomCenter;
    //
    // [SerializeField]
    // private Vector2 interactableDisplayOffset = Vector2.zero;
    
    //Rect and screen info
    private RectTransform _canvasRectTransform;

    private float _gameTimeLeft; //When set above 0, game over logic counted down
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (_canvasRectTransform == null) _canvasRectTransform = GetComponent<RectTransform>();
        
        //if no holder create one as first child
        if (iconHolder == null)
        {
            iconHolder = Instantiate(new GameObject("Icon Holder"), transform).transform;

            iconHolder.SetAsFirstSibling();
        }
        
        if (mainMenuPage == null) 
        {
            Debug.LogError( "Main Menu Page not set in UIManager, disabling UIManager" );
            
            enabled = false;
            
            return;
        }
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnStateChange;

        _gameTimeLeft = 0;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnStateChange;
    }

    private void Update()
    {
        if (_gameTimeLeft > 0)
        {
            _gameTimeLeft -= Time.deltaTime;
            
            SetTimer(_gameTimeLeft);
            
            if (_gameTimeLeft <= 0)
            {
                GameStateManager.Instance.GameOver(false);
            }
        }
    }
    
   private void SetTimer(float secondsLeft)
    {
        float minutes = Mathf.FloorToInt(secondsLeft / 60);
        
        float seconds = Mathf.FloorToInt(secondsLeft % 60);
        
        if (seconds < 10)
        {
            timeText.text = $"{minutes}:0{seconds}";
        }
        else
        {
            timeText.text = $"{minutes}:{seconds}";
        }
    }

    private void OnStateChange(EGameState toState, EGameState fromState)
    {
        switch (fromState)
        {
            case EGameState.Reset:
                
                // Turn off all UI
                mainMenuPage.SetActive(false);
                playerHUDPage.SetActive(false);
                pausePage.SetActive(false);
                gameOverPage.SetActive(false);
                
                break;
            
            case EGameState.MainMenu:
                
                // Turn off relevant UI
                mainMenuPage.SetActive(false);
                
                break;
            
            case EGameState.Game:
                
                // Turn off relevant UI
                playerHUDPage.SetActive(false);
                
                break;
            
            case EGameState.Pause:
                
                // Turn off relevant UI
                pausePage.SetActive(false);
                
                break;
            
            case EGameState.GameOver:
                // Turn off relevant UI
                gameOverPage.SetActive(false);
                break;
        }
        
        switch (toState)
        {
            case EGameState.Reset:
                break;
            
            case EGameState.MainMenu:
                
                // Turn on relevant UI
                mainMenuPage.SetActive(true);
                
                CursorLocked(false);
                
                break;
            
            case EGameState.Game:
                
                // Turn on relevant UI
                playerHUDPage.SetActive(true);
                
                CursorLocked(true);
                
                SetTimer(GameStateManager.Instance.GetStartTime());

                uiSelectWheel.SetToBase();
                
                break;
            
            case EGameState.Pause:
                
                // Turn on relevant UI
                pausePage.SetActive(true);
                
                CursorLocked(false);
                
                break;
            
            case EGameState.GameOver:
                // Turn on relevant UI
                gameOverPage.SetActive(true);

                CursorLocked(false);
                break;
        }
    }
    
    private void CursorLocked(bool locked)
    {
        Cursor.visible = !locked;
        
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
    
    public void StartCountdown(float time)
    {
        _gameTimeLeft = time;
    }
    
    public void CALL_UNPAUSE()
    {
        GameStateManager.Instance.EnterGameState(EGameState.Game, GameStateManager.Instance.GameStateSO.GameState);
    }
    
    public void CALL_SCENE_RELOAD(bool resetStage = true)
    {
        GameStateManager.Instance.ReloadScene(resetStage);
    }
    
    
    public void CALL_SCENE_LOAD(GameStateSceneInfo info)
    {
        GameStateManager.Instance.LoadSceneWithGameStateSceneInfo(info);
    }
    
    public void CALL_QUIT()
    {
        GameStateManager.Instance.QUIT_GAME();
    }
    
    public UISelectWheel UISelectWheel => uiSelectWheel;
}