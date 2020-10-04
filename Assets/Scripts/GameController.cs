using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

enum GameState
{
    Title,
    Game,
}

enum TitleState
{
    Reborn,
    Life,
    Death,
    Escape,
    Credits0,
    Credits1,
}

public class GameController : MonoBehaviour
{
    public float gameTitleOffset = 50f;

    public GameObject gamePrefab;
    public GameObject activeGame;
    public GameObject title;
    public GameObject continueText;
    public Text mainText;
    public AudioSource menuSound;

    public float hideTutorialAfterGameLongerThan = 10f;

    public string deathTitle = "you are dead";
    public string rebirthTitle = "you are reborn";
    public string lifeTitle = "you live a long and happy life";
    public string escapeText = "you have escaped";
    public string credits0Text = "a game by noah weiner";
    public string credits1Text = "thanks for playing";
    public float titleBeatTime = 0.5f;

    private GameState gameState;
    private TitleState titleState;
    private float gameTimer;
    private bool showTutorial;

    void Start()
    {
        gameState = GameState.Title;
        UpdateTitleState(TitleState.Death);
        showTutorial = true;
    }

    void UpdateState(GameState newState)
    {
        if (newState == GameState.Game)
        {
            if (activeGame != null)
            {
                Destroy(activeGame);
            }

            Camera.main.GetComponent<AudioListener>().enabled = false;

            var position = new Vector3(
                title.transform.position.x,
                title.transform.position.y + gameTitleOffset,
                0f);

            activeGame = Instantiate(gamePrefab, position, Quaternion.identity);

            activeGame.GetComponentInChildren<Rowboat>().OnRebirth += OnRebirth;
            activeGame.GetComponentInChildren<TheLight>().OnEscape += OnEscape;

            if (!showTutorial)
            {
                activeGame.GetComponentInChildren<Tutorial>().gameObject.SetActive(false);
            }


            gameTimer = 0f;
        }
        else if (newState == GameState.Title)
        {
            Camera.main.GetComponent<AudioListener>().enabled = true;

            var gameCamera = activeGame.GetComponentInChildren<CinemachineVirtualCamera>();
            var cameraPosition = gameCamera.transform.position;
            var river = activeGame.GetComponentInChildren<River>();
            var midpoint = river.GetMiddle(cameraPosition.y);

            title.transform.position = new Vector3(
                midpoint,
                cameraPosition.y - gameTitleOffset,
                title.transform.position.z);

            gameCamera.m_Priority = 0;

            StartCoroutine(DestroyActiveGameAfterCoroutine(4f));
        }

        gameState = newState;
    }

    void OnRebirth()
    {
        if (gameState != GameState.Game)
        {
            return;
        }

        UpdateState(GameState.Title);
        UpdateTitleState(TitleState.Reborn);
    }

    void OnEscape()
    {
        if (gameState != GameState.Game)
        {
            return;
        }

        UpdateState(GameState.Title);
        UpdateTitleState(TitleState.Escape);
        Destroy(activeGame);
    }

    void Update()
    {
        if (gameState == GameState.Title && Input.anyKeyDown)
        {
            switch (titleState)
            {
                case TitleState.Reborn:
                    ChangeTitle(TitleState.Life);
                    break;
                case TitleState.Life:
                    ChangeTitle(TitleState.Death);
                    break;
                case TitleState.Death:
                    UpdateState(GameState.Game);
                    break;
                case TitleState.Escape:
                    ChangeTitle(TitleState.Credits0);
                    break;
                case TitleState.Credits0:
                    ChangeTitle(TitleState.Credits1);
                    break;
            }
        }
        else if (gameState == GameState.Game)
        {
            gameTimer += Time.deltaTime;

            if (gameTimer > hideTutorialAfterGameLongerThan)
            {
                showTutorial = false;
            }
        }
    }

    void UpdateTitleState(TitleState state)
    {
        switch (state)
        {
            case TitleState.Death:
                mainText.text = deathTitle;
                break;
            case TitleState.Life:
                mainText.text = lifeTitle;
                break;
            case TitleState.Reborn:
                mainText.text = rebirthTitle;
                break;
            case TitleState.Escape:
                FlipTitleColors(true);
                mainText.text = escapeText;
                break;
            case TitleState.Credits0:
                mainText.text = credits0Text;
                break;
            case TitleState.Credits1:
                mainText.text = credits1Text;
                continueText.SetActive(false);
                break;
        }

        titleState = state;
    }

    void FlipTitleColors(bool isBlackOnWhite)
    {
        Camera.main.backgroundColor = isBlackOnWhite ? Color.white : Color.black;

        foreach (var text in title.GetComponentsInChildren<Text>())
        {
            text.color = isBlackOnWhite ? Color.black : Color.white;
        }
    }

    void ChangeTitle(TitleState newTitle)
    {
        StartCoroutine(ChangeTitleCoroutine(newTitle));
    }

    IEnumerator ChangeTitleCoroutine(TitleState newTitle)
    {
        foreach (Transform child in title.transform)
        {
            child.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(titleBeatTime);

        foreach (Transform child in title.transform)
        {
            child.gameObject.SetActive(true);
        }

        menuSound.Play();
        UpdateTitleState(newTitle);
    }

    IEnumerator DestroyActiveGameAfterCoroutine(float delay)
    {
        var game = activeGame;

        yield return new WaitForSeconds(delay);

        if (game != null)
        {
            game.SetActive(false);
        }
    }
}
