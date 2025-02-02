using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputAction joinAction; // Assigne cette action dans l'inspecteur
    private List<InputWrapper> joinedDevices = new List<InputWrapper>();
    public GameObject uiPlayer1Ready;
    public GameObject uiPlayer2Ready;
    public GameObject uiPlayer1Keyboard;
    public GameObject uiPlayer1Gamepad;
    public GameObject uiPlayer2Keyboard;
    public GameObject uiPlayer2Gamepad;
    public TextMeshProUGUI startCounter;
    public AudioClip menu, game;

    private GameState gameState = GameState.GameWaitingForPlayer;

    void Start()
    {
        uiPlayer1Ready.SetActive(false);
        uiPlayer2Ready.SetActive(false);
        startCounter.gameObject.SetActive(false);
        AudioController.PlayMusic(menu);
    }

    private void OnEnable()
    {
        joinAction.Enable();
        joinAction.performed += OnJoin;
    }

    private void OnDisable()
    {
        joinAction.performed -= OnJoin;
        joinAction.Disable();
    }

    private void OnJoin(InputAction.CallbackContext context)
    {
        if (gameState != GameState.GameWaitingForPlayer)
        {
            return;
        }
        InputDevice device = context.control.device;
        InputWrapper inputWrapper = new InputWrapper(device);
        var isAlreadyPresent = joinedDevices.Any(x => x.device == device);
        if (!isAlreadyPresent)
        {
            joinedDevices.Add(inputWrapper);
            int playerNumber = joinedDevices.Count;
            Debug.Log("Player " + playerNumber + " a rejoint avec " + device.displayName);
            if (playerNumber == 1)
            {
                if (inputWrapper.inputType == InputType.GamePad)
                {
                    uiPlayer1Gamepad.SetActive(true);
                }
                else
                {
                    uiPlayer1Keyboard.SetActive(true);
                }
                uiPlayer1Ready.SetActive(true);
            }
            else if (playerNumber == 2)
            {
                if (inputWrapper.inputType == InputType.GamePad)
                {
                    uiPlayer2Gamepad.SetActive(true);
                }
                else
                {
                    uiPlayer2Keyboard.SetActive(true);
                }
                uiPlayer2Ready.SetActive(true);
            }
        }
        if (joinedDevices.Count == 2)
        {
            gameState = GameState.GamePlayersReady;
            StartCoroutine(StartGame());
        }
    }

    private System.Collections.IEnumerator gameIsStarted()
    {
        gameState = GameState.GameStarted;
        yield return SceneManager.LoadSceneAsync("Scene0");
        Debug.Log("Scene is loaded");
        AudioController.PlayMusic(game);
        Debug.Log(PlayerInput.all);
        var player2 = PlayerInput.GetPlayerByIndex(0);
        var player1 = PlayerInput.GetPlayerByIndex(1);

        player1.user.UnpairDevices();
        var firstDevice = getFirstDevice();
        player1.SwitchCurrentControlScheme(firstDevice.inputType == InputType.GamePad ? "Gamepad" : "Keyboard&Mouse", firstDevice.device);
        
        player2.user.UnpairDevices();
        var secondDevice = getSecondDevice();
        player2.SwitchCurrentControlScheme(secondDevice.inputType == InputType.GamePad ? "Gamepad" : "Keyboard&Mouse", secondDevice.device);
    }

    private System.Collections.IEnumerator StartGame()
    {
        startCounter.gameObject.SetActive(true);
        startCounter.text = "3";
        yield return new WaitForSeconds(1);
        startCounter.text = "2";
        yield return new WaitForSeconds(1);
        startCounter.text = "1";
        yield return new WaitForSeconds(1);
        startCounter.text = "GO!";
        yield return new WaitForSeconds(1);
        Debug.Log("Game started");
        StartCoroutine(gameIsStarted());
    }

    public InputWrapper getFirstDevice() {
        Debug.Log("First Device = " + joinedDevices.First());
        return joinedDevices.First();
    }

    public InputWrapper getSecondDevice() {
        Debug.Log("Second Device = " + joinedDevices.Last());
        return joinedDevices.Last();
    }
}
