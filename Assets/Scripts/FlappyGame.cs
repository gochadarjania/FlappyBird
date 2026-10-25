using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FlappyGame : MonoBehaviour
{
    public Rigidbody2D bird;
    public SpriteRenderer birdImage;
    public Sprite[] wingFrames;
    public GameObject pipePrefab;
    public Text scoreText, titleText, detailText, buttonText;
    public GameObject panel;
    public AudioSource audioSource;
    public AudioClip flapSound, hitSound, pointSound;
    public float flapSpeed = 4.8f, pipeSpeed = 1.45f, spawnInterval = 2.1f;
    public bool IsPlaying { get; private set; }
    private bool ended;
    private float timer, animationTime;
    private int score;

    void Start()
    {
        Time.timeScale = 1;
        float relativeWidth = (9f / 16f) / ((float)Screen.width / Screen.height);
        Camera.main.rect = relativeWidth < 1
            ? new Rect((1 - relativeWidth) / 2, 0, relativeWidth, 1)
            : new Rect(0, (1 - 1 / relativeWidth) / 2, 1, 1 / relativeWidth);
        bird.simulated = false;
        titleText.text = "FLAPPY BIRD";
        detailText.text = "SPACE / CLICK TO FLAP\nFly through the gaps";
        buttonText.text = "PLAY";
        scoreText.text = "0";
    }

    public void PlayOrRestart()
    {
        if (ended) { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); return; }
        if (IsPlaying) return;
        IsPlaying = true;
        panel.SetActive(false);
        bird.simulated = true;
        timer = 0.7f;
        Flap();
    }

    void Update()
    {
        if (!IsPlaying) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) Flap();
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            GameObject pair = Instantiate(pipePrefab, new Vector3(2.1f, Random.Range(-0.3f, 0.85f), 0), Quaternion.identity);
            pair.GetComponent<PipePair>().game = this;
            timer = spawnInterval;
        }
        animationTime += Time.deltaTime;
        birdImage.sprite = wingFrames[(int)(animationTime * 10) % wingFrames.Length];
        birdImage.transform.rotation = Quaternion.Euler(0, 0, Mathf.Clamp(bird.linearVelocity.y * 6, -65, 25));
        if (bird.position.y > 2.65f || bird.position.y < -1.95f) EndGame();
    }

    void Flap()
    {
        bird.linearVelocity = new Vector2(0, flapSpeed);
        audioSource.PlayOneShot(flapSound, 0.5f);
    }

    public void AddScore()
    {
        if (!IsPlaying) return;
        score++;
        scoreText.text = score.ToString();
        audioSource.PlayOneShot(pointSound, 0.45f);
    }

    public void EndGame()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        ended = true;
        bird.linearVelocity = Vector2.zero;
        bird.simulated = false;
        audioSource.PlayOneShot(hitSound, 0.5f);
        int best = Mathf.Max(score, PlayerPrefs.GetInt("FlappyBest", 0));
        PlayerPrefs.SetInt("FlappyBest", best);
        PlayerPrefs.Save();
        titleText.text = "GAME OVER";
        detailText.text = "SCORE  " + score + "\nBEST  " + best;
        buttonText.text = "RESTART";
        panel.SetActive(true);
    }

    void OnCollisionEnter2D(Collision2D collision) { EndGame(); }
}
