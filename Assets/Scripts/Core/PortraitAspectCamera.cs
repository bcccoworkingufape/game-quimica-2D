using UnityEngine;
#if UNITY_STANDALONE || UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace Core
{
    /// <summary>
    /// Mantém o conteúdo do jogo na proporção retrato em telas widescreen (PC),
    /// ajustando <see cref="Camera.rect"/> para criar barras pretas (pillarbox) nas
    /// laterais e centralizar o jogo. Em telas mais altas que o alvo aplica letterbox
    /// (barras em cima/baixo).
    ///
    /// Em Android/iOS todo o corpo é removido pela diretiva de pré-processador, então
    /// o comportamento mobile NÃO é alterado de forma alguma.
    ///
    /// Não é necessário editar cenas nem prefabs: o componente se auto-instala na
    /// Camera.main de cada cena via <see cref="RuntimeInitializeOnLoadMethod"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class PortraitAspectCamera : MonoBehaviour
    {
        // Proporção alvo (retrato) = mesma do dispositivo de referência do jogo:
        // Google Pixel 5 -> 1080 x 2340 (9:19.5)
        [Header("Proporção alvo (retrato)")]
        [SerializeField] private float targetWidth = 1080f;
        [SerializeField] private float targetHeight = 2340f;

        [Header("Barras")]
        [SerializeField] private Color barColor = Color.black;

#if UNITY_STANDALONE || UNITY_EDITOR
        private Camera _camera;
        private Camera _backgroundCamera;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        /// <summary>
        /// Auto-instala o componente na Camera.main, sem precisar editar cena/prefab.
        /// Roda após cada carregamento de cena.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            AttachToMainCamera();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToMainCamera();

        private static void AttachToMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            if (cam.GetComponent<PortraitAspectCamera>() == null)
            {
                cam.gameObject.AddComponent<PortraitAspectCamera>();
            }
        }

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            Apply();
        }

        private void OnDisable()
        {
            // Restaura o viewport cheio e descarta a câmera de fundo.
            if (_camera != null)
            {
                _camera.rect = new Rect(0f, 0f, 1f, 1f);
            }

            if (_backgroundCamera != null)
            {
                Destroy(_backgroundCamera.gameObject);
                _backgroundCamera = null;
            }
        }

        private void Update()
        {
            // Reaplica apenas quando a resolução/janela muda (redimensionar no PC).
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (_camera == null)
            {
                return;
            }

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            float targetAspect = targetWidth / targetHeight;          // 1080/2340 = 0.4615
            float windowAspect = (float)Screen.width / Screen.height; // largura/altura atual
            float scaleHeight = windowAspect / targetAspect;

            Rect rect = new Rect(0f, 0f, 1f, 1f);

            if (scaleHeight < 1f)
            {
                // Janela mais "estreita/alta" que o alvo -> letterbox (barras topo/base).
                rect.height = scaleHeight;
                rect.y = (1f - scaleHeight) * 0.5f;
            }
            else
            {
                // Janela mais larga que o alvo (PC widescreen) -> pillarbox (barras laterais).
                float scaleWidth = 1f / scaleHeight;
                rect.width = scaleWidth;
                rect.x = (1f - scaleWidth) * 0.5f;
            }

            _camera.rect = rect;
            EnsureBackgroundCamera();
        }

        /// <summary>
        /// Cria (uma vez) uma câmera de fundo que pinta a tela inteira de preto atrás
        /// da câmera principal. Isso evita artefatos de "smearing" nas áreas fora do
        /// <see cref="Camera.rect"/>, que de outra forma não são limpas a cada frame.
        /// </summary>
        private void EnsureBackgroundCamera()
        {
            if (_backgroundCamera != null)
            {
                return;
            }

            var go = new GameObject("PortraitLetterboxBackground");
            go.transform.SetParent(transform, false);

            _backgroundCamera = go.AddComponent<Camera>();
            _backgroundCamera.cullingMask = 0;                 // não renderiza objetos
            _backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
            _backgroundCamera.backgroundColor = barColor;
            _backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f); // tela inteira
            _backgroundCamera.depth = _camera.depth - 1;       // renderiza ATRÁS da principal
            _backgroundCamera.orthographic = _camera.orthographic;
            _backgroundCamera.allowHDR = false;
            _backgroundCamera.allowMSAA = false;
            _backgroundCamera.useOcclusionCulling = false;

            // Garante que esta câmera auxiliar nunca carregue um AudioListener extra.
            var listener = go.GetComponent<AudioListener>();
            if (listener != null)
            {
                Destroy(listener);
            }
        }
#endif
    }
}
