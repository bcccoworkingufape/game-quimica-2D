using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Encapsula o carregamento de cenas com fade e o historico de cenas.
    /// Extraido do <see cref="GameManager"/> para isolar a responsabilidade
    /// de "roteamento" (Single Responsibility Principle).
    /// </summary>
    public class SceneRouter
    {
        private readonly MonoBehaviour _coroutineHost;
        private readonly SceneFader _fader;
        private readonly float _fadeDuration;
        private readonly Stack<string> _history = new Stack<string>();

        public SceneRouter(MonoBehaviour coroutineHost, SceneFader fader, float fadeDuration)
        {
            _coroutineHost = coroutineHost;
            _fader = fader;
            _fadeDuration = fadeDuration;
        }

        public void NotifySceneLoaded()
        {
            if (_fader != null && _fader.Alpha > 0.01f)
                _coroutineHost.StartCoroutine(_fader.FadeIn(_fadeDuration));
        }

        public void LoadScene(string sceneName)
        {
            _coroutineHost.StartCoroutine(LoadSceneWithFade(sceneName));
        }

        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            if (_fader != null)
                yield return _coroutineHost.StartCoroutine(_fader.FadeOut(_fadeDuration));

            string current = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(current))
                _history.Push(current);

            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;
        }

        public void GoBack()
        {
            _coroutineHost.StartCoroutine(GoBackWithFade());
        }

        private IEnumerator GoBackWithFade()
        {
            if (_history.Count == 0)
            {
                Debug.LogWarning("[SceneRouter] Não ha cenas no historico para voltar.");
                yield break;
            }

            if (_fader != null)
                yield return _coroutineHost.StartCoroutine(_fader.FadeOut(_fadeDuration));

            string previous = _history.Pop();
            var op = SceneManager.LoadSceneAsync(previous);
            while (!op.isDone) yield return null;
        }
    }
}
