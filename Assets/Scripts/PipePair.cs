using UnityEngine;

public class PipePair : MonoBehaviour
{
    [HideInInspector] public FlappyGame game;
    private bool scored;

    void Update()
    {
        if (game == null || !game.IsPlaying) return;
        transform.position += Vector3.left * game.pipeSpeed * Time.deltaTime;
        // Count only after the trailing edge has cleared the bird.
        if (!scored && transform.position.x + 0.26f < game.bird.position.x - 0.12f)
        {
            scored = true;
            game.AddScore();
        }
        if (transform.position.x < -2.4f) Destroy(gameObject);
    }
}
