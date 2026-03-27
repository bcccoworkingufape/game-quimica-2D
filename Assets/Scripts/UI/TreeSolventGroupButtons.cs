using TMPro;
using UnityEngine;

public class TreeSolventGroupButtons : MonoBehaviour
{
    [SerializeField]
    private GameObject Header;
    [SerializeField]
    private GameObject Message;

    public void updateMessage(string solventGroupType)
    {
        switch (solventGroupType)
        {
            case "S1":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos polares neutros";
                Message.GetComponent<TextMeshProUGUI>().text = "Álcoois, aldeídos, cetonas, ésteres, nitrilas e amidas monofuncionais com 5 átomos de carbono ou menos";
                break;

            case "S2":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos muito polares";
                Message.GetComponent<TextMeshProUGUI>().text = "Sais de ácidos orgânicos, sais de amônio (aminas protonadas), aminoácidos, compostos polifuncionais (carboidratos, poliálcoois, ácidos, etc.)";

                break;

            case "SA":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos polares de caráter ácido";
                Message.GetComponent<TextMeshProUGUI>().text = "Ácidos monocarboxílicos com 5 átomos de carbono ou menos, ácidos arenossulfônicos";
                break;

            case "SB":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos polares de caráter básico";
                Message.GetComponent<TextMeshProUGUI>().text = "Aminas monofuncionais com 6 átomos de carbono ou menos";
                break;

            case "A1":
                Header.GetComponent<TextMeshProUGUI>().text = "Ácidos orgânicos fortes e apolares";
                Message.GetComponent<TextMeshProUGUI>().text = "Ácidos carboxílicos, fenóis com grupos eletrofílicos em orto e para, β-dicetonas";
                break;

            case "A2":
                Header.GetComponent<TextMeshProUGUI>().text = "Ácidos orgânicos fracos e apolares";
                Message.GetComponent<TextMeshProUGUI>().text = "Fenóis, enóis, oximas, imidas, sulfonamidas, tiofenóis com mais de 5 átomos de carbono, nitro-compostos com hidrogênio alfa.";
                break;

            case "B":
                Header.GetComponent<TextMeshProUGUI>().text = "Bases orgânicas e apolares";
                Message.GetComponent<TextMeshProUGUI>().text = "Aminas com 8 ou mais átomos de carbono, anilinas; alguns oxiéteres";
                break;

            case "N1":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos oxigenados e apolares";
                Message.GetComponent<TextMeshProUGUI>().text = "Álcoois, aldeídos, metil-cetonas, cetonas cíclicas e ésteres contendo somente um grupo funcional e número de átomos de carbono entre 5 e 9; éteres com menos de 8 átomos de carbono; epóxidos";
                break;

            case "N2":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos insaturados e apolares";
                Message.GetComponent<TextMeshProUGUI>().text = "Alcenos, alcinos, alguns compostos aromáticos com grupos ativantes, algumas cetonas";
                break;

            case "I":
                Header.GetComponent<TextMeshProUGUI>().text = "Compostos inertes e apolares";
                Message.GetComponent<TextMeshProUGUI>().text = "Hidrocarbonetos saturados, halogeno-alcanos, haletos de arila, éteres diarílicos, compostos aromáticos desativados";
                break;

            default:
                Header.GetComponent<TextMeshProUGUI>().text = "";
                Message.GetComponent<TextMeshProUGUI>().text = "";
                break;
        }
    }
}
