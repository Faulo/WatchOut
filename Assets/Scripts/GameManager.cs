using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private enum GameScenes {Splash, Game, Finished};
	private GameScenes currentScene = GameScenes.Splash;
	private static GameManager _instance;

	public static GameManager Instance { get { return _instance; } }


	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			_instance = this;
		}
		DontDestroyOnLoad(this.gameObject);
	}


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.F1))
		{
			LoadSplashScreen();
		}
		else if (Input.GetKey (KeyCode.F2))
		{
			LoadGameScene();
		}
		else if (Input.GetKey(KeyCode.F3))
		{
			LoadFinisehdVersion();
		} else if (currentScene == GameScenes.Splash && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
		{
			LoadGameScene();
		}
	}

	public void LoadGameScene()
	{
		SceneManager.LoadScene(1);
	}

	public void LoadFinisehdVersion()
	{
		SceneManager.LoadScene(2);
	}

	public void LoadSplashScreen()
	{
		SceneManager.LoadScene(0);
	}
}
