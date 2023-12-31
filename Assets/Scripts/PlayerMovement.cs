using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float horizontalSpeed;

    [SerializeField] private GameObject waterFlip;
    
    [Header("Jump Property")]
    [SerializeField] private float waterHeight;
    [SerializeField] private float maxJumpHeight;
    [SerializeField] private float flyingTime;

    [SerializeField] private AnimationCurve jumpCurve;

    private Animator playerAnim;

    private float jumpBuffer;
    
    private float maxPosX;
    private float minPosX;
    private float minPosY;

    private bool isJumping;
    private bool isDash = false;
    public GameObject dashEffect;

    private PlayerScore playerScore;
    private PlayerObstacle playerObstacle;

    public float dashTimePer;

    public float increaseScrollSpeed;
    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        playerObstacle = GetComponent<PlayerObstacle>();
        playerScore = GetComponent<PlayerScore>();
        playerAnim = GetComponent<Animator>();
        dashEffect.SetActive(false);

        maxPosX = Camera.main.ViewportToWorldPoint(new Vector3(0.99f, 0)).x - col.bounds.size.x / 2;
        minPosX = Camera.main.ViewportToWorldPoint(new Vector3(0.01f, 0f)).x + col.bounds.size.x / 2;
        minPosY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.01f)).y + col.bounds.size.y / 2;
    }

    public void Move(float h, float v)
    {
        if (isJumping) return;

        if (h < 0f)
            playerAnim.SetBool("left", true);
        else if (h > 0f)
            playerAnim.SetBool("left", false);

        Vector2 nomal = new Vector2(h, v).normalized;

        float speedScale = Mathf.Min(1 + (GameManager.Instance.GetIncreasementSpeed() - 1) * 0.6f, 1.22f);
        
        float playerPosX = Mathf.Clamp(transform.position.x + nomal.x * horizontalSpeed * speedScale * Time.deltaTime, minPosX, maxPosX);
        float playerPosY = Mathf.Clamp(transform.position.y + nomal.y * verticalSpeed * speedScale * Time.deltaTime, minPosY, waterHeight);
        
        if(playerPosY >= waterHeight && v > 0)
        {
            jumpBuffer += Time.deltaTime;
            if(jumpBuffer > 0.06f)
            {
                StartCoroutine(Jump());
                isJumping = true;
                return;
            }
        }
        else
        {
            jumpBuffer = 0;
        }
        transform.position = new Vector3(playerPosX, playerPosY, 0);
        
        
    }


	IEnumerator Jump()
    {
        playerAnim.SetTrigger("jump");

        float upDownTime = 0.34f;
        float timer = 0;
        
        float offset = maxJumpHeight - waterHeight;
        GameObject wf1 = Instantiate(waterFlip, transform.position - new Vector3(0, 0.25f, 0), Quaternion.identity);
        wf1.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        while (timer < upDownTime)
        {
            Dash();

            if (isDash&&playerScore.player_CurrentFish !=0) { break; }
            timer += Time.deltaTime;
            
            float currentPosY = jumpCurve.Evaluate(timer / upDownTime) * offset;
            transform.position = new Vector3(transform.position.x, currentPosY + waterHeight);
            yield return null;
        }
        
        while (timer > 0)
        {
            Dash();
			if (isDash) timer += Time.deltaTime;
			timer -= Time.deltaTime;

            timer = Mathf.Min(timer, maxJumpHeight);
            
            float currentPosY = jumpCurve.Evaluate(timer / upDownTime) * offset;
            transform.position = new Vector3(transform.position.x, currentPosY + waterHeight);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, waterHeight);
        GameObject wf2 = Instantiate(waterFlip, transform.position - new Vector3(0, 0.25f, 0), Quaternion.identity);
        wf2.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        isJumping = false;
    }


    public void Dash()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!isDash&& playerScore.player_CurrentFish!=0)
                StartCoroutine(StartDash());
        }
    }

    IEnumerator StartDash()
	{
        SoundManager.Instance.Dash();
        dashEffect.SetActive(true);
        playerAnim.SetBool("dash", true);
		float dashTime = playerScore.player_CurrentFish / dashTimePer;
        //float tempTime = GameManager.Instance.GetIncreasementSpeed();

		GameManager.Instance.IncreaseSpeedRatio(increaseScrollSpeed);
		isDash = true;
        playerObstacle.isDash = true;

        float fishFill = playerScore.fishBar.fillAmount;
        float maxDashTime = dashTime;

        float timer = 0;
        while (dashTime > 0f)
        {
            timer += Time.deltaTime;
            
            dashTime -= Time.deltaTime;
            if(timer > 0.1f)
            {
                playerScore.fishScore += 2;
                timer = 0;
            }
			playerScore.fishBar.fillAmount = Mathf.Lerp(0f, fishFill, dashTime / maxDashTime);
			yield return null;
		}

		GameManager.Instance.InitFastSpeedRatio();

        SoundManager.Instance.DashEnd();
        playerAnim.SetBool("dash", false);
        dashEffect.SetActive(false);
        isDash = false;
        playerObstacle.isDash = false;
        playerScore.player_CurrentFish = 0;
	}
}
