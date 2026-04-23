using UnityEngine;
using System.Collections;

public class SimpleSpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] animationFrames;
    private int currentFrame;
    public float fps = 10f;
    private Coroutine animationRoutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Erro: Adicione um SpriteRenderer ao GameObject " + gameObject.name);
        }
    }

    // Método que você chamará via código passando os dados do JSON
    public void LoadAndPlayAnimation(string flask, string mixture, string resultType, string litmus)
    {
        // Monta o caminho: ex FLASK_01/LIQUID_LIQUID/InsolubleFloat/None
        string path = flask + "/" + mixture + "/" + resultType + "/" + litmus;

        Sprite[] frames = Resources.LoadAll<Sprite>(path);

        if (frames != null && frames.Length > 0)
        {
            PlayAnimation(frames);
        }
        else
        {
            Debug.LogWarning("Spritesheet nao encontrado em: Resources/" + path);
        }
    }

    private void PlayAnimation(Sprite[] frames)
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationFrames = frames;
        animationRoutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        currentFrame = 0;
        while (true)
        {
            if (animationFrames.Length > 0)
            {
                spriteRenderer.sprite = animationFrames[currentFrame];
                currentFrame = (currentFrame + 1) % animationFrames.Length;
            }
            yield return new WaitForSeconds(1f / fps);
        }
    }
}