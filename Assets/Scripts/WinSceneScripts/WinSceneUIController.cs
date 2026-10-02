using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinSceneUI_Controller : MonoBehaviour
{
    [SerializeField] private Image[] stars;
    [SerializeField] private Sprite emptyStar;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI recordText;

    private void Start()
    {
        var gm = GameManager.Instance;
        var run = gm != null ? gm.RunScore : null;
        bool completed = run != null && run.Completed;
        float count = completed ? run.Stars : 0;
        for (int i = 0; stars != null && i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            stars[i].gameObject.SetActive(i < 5);
            if (i >= 5) continue;

            float fill = Mathf.Clamp01(count - i);
            if (fill < 1f)
            {
                Sprite fullStar = stars[i].sprite;
                if (emptyStar != null) stars[i].sprite = emptyStar;
                else stars[i].color = new Color(1, 1, 1, .2f);
                if (fill > 0f)
                {
                    // Sobrepõe a metade esquerda preenchida ao contorno vazio.
                    var overlay = new GameObject("HalfStarFill", typeof(RectTransform), typeof(Image));
                    overlay.transform.SetParent(stars[i].transform, false);
                    var rect = (RectTransform)overlay.transform;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var image = overlay.GetComponent<Image>();
                    image.sprite = fullStar;
                    image.preserveAspect = stars[i].preserveAspect;
                    image.raycastTarget = false;
                    image.type = Image.Type.Filled;
                    image.fillMethod = Image.FillMethod.Horizontal;
                    image.fillOrigin = (int)Image.OriginHorizontal.Left;
                    image.fillAmount = fill;
                }
            }
        }
        if (titleText != null) titleText.text = completed ? "Fase concluída!" : "Sem classificação";
        if (summaryText != null)
        {
            summaryText.enableAutoSizing = true;
            summaryText.fontSizeMax = 44;
            summaryText.text = completed
                ? $"{count} de 5 estrelas\nPrimeira tentativa: {run.FirstAttemptCorrect}/{gm.TotalQuestionsInRun} • Erros: {run.Errors}\nPontos: {run.Score}/100"
                : "Conclua todas as questões para receber estrelas.";
        }
        if (gm != null && gm.CurrentDifficulty != null)
        {
            if (difficultyText != null) difficultyText.text = gm.CurrentDifficulty.difficultyName;
            if (modeText != null) modeText.text = gm.CurrentDifficulty.ModeLabel;
        }
        if (progressText != null) progressText.text = completed ? "100%" : "—";
        if (recordText != null) recordText.text = completed ? $"Total: {StarRecords.Total(run.Mode)} estrelas" : "";
    }

    public void LoadScene(string nomeDaCena)
    {
        if (string.IsNullOrWhiteSpace(nomeDaCena) || GameManager.Instance == null) return;
        if (nomeDaCena == SceneNames.Lab) GameManager.Instance.StartGame();
        else GameManager.Instance.LoadScene(nomeDaCena);
    }
    public void JogarNovamente() => LoadScene(SceneNames.Lab);
    public void VoltarAoMenuInicial() => LoadScene(SceneNames.Menu);
}

