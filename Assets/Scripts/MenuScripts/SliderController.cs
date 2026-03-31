using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Data;
using Core;

namespace MenuScripts
{
    public class Slider_Controller : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;  // O GameObject "Fill" do Slider
        [SerializeField] private Sprite[] progressSprites; // Array de sprites (0-5)

        private NavbarController navbarController;

        private int progress = 0;

        private void Start()
        {
            StartCoroutine(LoadProgressBar());
        }

        public void IncreaseProgress()
        {
            progress = Mathf.Clamp(progress + 1, 0, progressSprites.Length - 1);
            slider.value = progress;
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            fillImage.sprite = progressSprites[progress];
        }

        // Atualizar a barra de progresso a cada segundo
        private IEnumerator LoadProgressBar()
        {
            for (int i = 0; i < 6; i++)
            {
                IncreaseProgress();
                yield return new WaitForSeconds(0.8f); // tempo em segundos
            }

            navbarController.OnClickHome();
        }
    }
}
