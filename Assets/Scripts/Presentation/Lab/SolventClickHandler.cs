using UnityEngine;
using Presentation.Lab;
using Core.Audio;

public class SolventClickHandler : MonoBehaviour
{
    [Header("Configuração do solvente")]
    [SerializeField] private int solventId;
    [SerializeField] private string solventName;

    [Header("Referência")]
    [SerializeField] private MixingRoundController mixingRoundController;

    private void Awake()
    {
        if (mixingRoundController == null)
        {
            mixingRoundController = FindAnyObjectByType<MixingRoundController>();
        }
    }

    private void OnMouseDown()
    {
        // Clique direto no objeto (2D/3D)
        TriggerMix();
    }

    /// <summary>
    /// Método público para ser chamado pelo botão de UI (OnClick).
    /// </summary>
    public void OnSolventButtonClick()
    {
        // Clique em botão da UI
        TriggerMix();
    }

    private void TriggerMix()
    {
        if (mixingRoundController == null)
        {
            Debug.LogWarning("[SolventClickHandler] MixingRoundController não atribuído.");
            return;
        }

        SfxManager.Instance?.PlayButtonClick();
        mixingRoundController.OnSolventClicked(solventId, solventName);
    }
}