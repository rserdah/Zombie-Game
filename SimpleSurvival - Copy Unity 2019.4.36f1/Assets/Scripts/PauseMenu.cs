using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : UIScreen
{
    private Button[] buttons;
    private AudioSource audioSource;
    public AudioClip typewriterSound;


    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        Transform t = TransformHelper.FindRecursive(transform, "t_buttons");
        buttons = t.GetComponentsInChildren<Button>();
        foreach(Button b in buttons)
        {
            switch(b.gameObject.name)
            {
                case "b_resume":
                    b.onClick.AddListener(Resume);
                    break;

                case "b_map":
                    b.onClick.AddListener(Map);
                    break;

                case "b_game":
                    b.onClick.AddListener(Game_);
                    break;

                case "b_settings":
                    b.onClick.AddListener(Settings);
                    break;

                case "b_quit":
                    b.onClick.AddListener(Quit);
                    break;

                case "b_quittodesktop":
                    b.onClick.AddListener(QuitToDesktop);
                    break;

                default:
                    Debug.LogError($"Button { b.gameObject.name } does not have a corresponding function");
                    break;
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Resume();

        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            audioSource.PlayOneShot(typewriterSound);
    }

    private void Pause()
    {
        gameObject.SetActive(true);
        //Game.paused = true;
    }

    private void Resume()
    {
        Game.paused = false;
        gameObject.SetActive(false);
    }

    private void Map()
    {

    }

    private void Game_()
    {

    }

    private void Settings()
    {

    }

    private void Quit()
    {

    }

    private void QuitToDesktop()
    {

    }
}
