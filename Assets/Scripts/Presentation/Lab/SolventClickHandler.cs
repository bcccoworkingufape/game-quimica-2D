using UnityEngine;
using Presentation.Lab;

public class SolventClickHandler : MonoBehaviour
{
    [Header("Configuração do solvente")]
    [SerializeField] private int solventId;
    [SerializeField] private string solventName;

    [Header("Referência")]
    [SerializeField] private TestManager testManager;

    private void Awake()
    {
        if (testManager == null)
        {
            testManager = FindObjectOfType<TestManager>();
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
        if (testManager == null)
        {
            Debug.LogWarning("[SolventClickHandler] TestManager não atribuído.");
            return;
        }

        testManager.OnSolventClicked(solventId, solventName);
    }
}
