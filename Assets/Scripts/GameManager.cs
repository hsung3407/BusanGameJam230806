using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    [Range(1f, 10f)]
    public float speedScale;
    private float timer;

    private float speedIncreasement;
    private float speedNormal;
    private float speedFast;
    private float speedSlow;
    public static GameManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (instance == null)
                    Debug.Log("no Singleton obj");
            }
            return instance;
        }
    }
     
    public float GetIncreasementSpeed() { return speedIncreasement; }
    public void DecreaseSpeedRatio(float ratio) { speedIncreasement = speedNormal; speedIncreasement *= ratio; }
    public void IncreaseSpeedRatio(float ratio) { speedIncreasement = speedNormal; speedIncreasement /= ratio; }
    public void ChangeSpeedRatio() { speedIncreasement = speedNormal; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        speedIncreasement = 1f;
        speedNormal = 1f;
		timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.5f)
        {
            speedIncreasement += Time.deltaTime * speedScale;
			speedNormal += Time.deltaTime * speedScale;
			timer = 0f;
        }
        
        if(Input.GetKey(KeyCode.Escape)) GameOverMenu.Instance.GameOver();

    }
}
