using UnityEngine;
using Domain;
using System.Collections.Generic;

namespace Presentation.Lab
{
    public enum AnimationMode
    {
        /// <summary>
        /// Usa parâmetros do Animator e transições do AnyState (padrão).
        /// </summary>
        UseParameters,

        /// <summary>
        /// Usa CrossFade diretamente com o nome do estado.
        /// Use se as transições do AnyState não estiverem funcionando.
        /// </summary>
        UseCrossFade
    }

    /// <summary>
    /// Controlador que atualiza os parâmetros do Animator com base no SolubilityOutcome.
    /// Liga o resultado da mistura à animação correta do frasco.
    /// 
    /// Mapeamento dos parâmetros do Animator:
    /// - flaskType:        FLASK_01=0, FLASK_02=1, FLASK_03=2, FLASK_04=3
    /// - mixtureType:      LL=0, SL=1
    /// - solubilityResult: Soluble=0, InsolubleFloat=1, InsolubleSink=2
    /// - litmusResult:     None=0, Basic=1, Neutral=2, Acidic=3
    /// </summary>
    public class FlaskAnimationController : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Animator do frasco que contém o Flask Controller")]
        [SerializeField] private Animator flaskAnimator;

        [Header("Carregar Controller Automaticamente")]
        [Tooltip("Caminho do AnimatorController em Resources (sem extensão)")]
        [SerializeField] private string controllerResourcePath = "Sprites/Flasks/Flask Controller";

        [Header("Modo de Animação")]
        [Tooltip("UseParameters: usa transições do AnyState (recomendado). UseCrossFade: usa Play direto com nome do estado.")]
        [SerializeField] private AnimationMode animationMode = AnimationMode.UseCrossFade;

        [Header("Nomes dos Parâmetros (devem bater com o Animator Controller)")]
        [SerializeField] private string flaskTypeParam = "flaskType";
        [SerializeField] private string mixtureTypeParam = "mixtureType";
        [SerializeField] private string solubilityResultParam = "solubilityResult";
        [SerializeField] private string litmusResultParam = "litmusResult";

        [Header("Debug")]
        [SerializeField] private bool logAnimationKeys = true;

        private void Awake()
        {
            ValidateAndSetupAnimator();
        }

        /// <summary>
        /// Valida se o Animator está configurado corretamente.
        /// Tenta carregar o controller de Resources se necessário.
        /// </summary>
        private void ValidateAndSetupAnimator()
        {
            // Tenta encontrar o Animator no mesmo GameObject se não estiver atribuído
            if (flaskAnimator == null)
            {
                flaskAnimator = GetComponent<Animator>();
            }

            if (flaskAnimator == null)
            {
                Debug.LogError("[FlaskAnimationController] Animator não encontrado. " +
                               "Atribua um Animator no Inspector ou adicione o componente Animator neste GameObject.");
                return;
            }

            // Verifica se o Animator tem um controller
            if (flaskAnimator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("[FlaskAnimationController] Animator não tem um Controller atribuído. " +
                                 $"Tentando carregar de Resources: '{controllerResourcePath}'");

                var controller = Resources.Load<RuntimeAnimatorController>(controllerResourcePath);
                if (controller != null)
                {
                    flaskAnimator.runtimeAnimatorController = controller;
                    Debug.Log("[FlaskAnimationController] Controller carregado com sucesso de Resources.");
                }
                else
                {
                    Debug.LogError($"[FlaskAnimationController] Falha ao carregar Controller de '{controllerResourcePath}'. " +
                                   "Verifique se o arquivo 'Flask Controller.controller' está em Assets/Resources/Sprites/Flasks/");
                }
            }
        }

        /// <summary>
        /// Atualiza os parâmetros do Animator com base no SolubilityOutcome.
        /// Chame este método após obter o resultado da mistura.
        /// IMPORTANTE: O painel deve estar sempre ativo (use SolutionPanelAnimator para controlar visibilidade via escala).
        /// </summary>
        /// <param name="outcome">Resultado da solubilidade vindo do MixSolutionUseCase</param>
        public void SetAnimationFromOutcome(SolubilityOutcome outcome)
        {
            if (outcome == null)
            {
                Debug.LogError("[FlaskAnimationController] SolubilityOutcome é nulo.");
                return;
            }

            if (flaskAnimator == null)
            {
                Debug.LogError("[FlaskAnimationController] Animator não atribuído no Inspector.");
                return;
            }

            // Verifica se o Animator tem um controller válido antes de tentar definir parâmetros
            if (flaskAnimator.runtimeAnimatorController == null)
            {
                Debug.LogError("[FlaskAnimationController] O Animator não tem um RuntimeAnimatorController! " +
                               "Arraste o 'Flask Controller' para o campo Controller do Animator no Inspector, " +
                               "ou verifique se o arquivo está em Resources/Sprites/Flasks/");
                return;
            }

            // Verifica se o GameObject está ativo
            if (!flaskAnimator.gameObject.activeInHierarchy)
            {
                Debug.LogError("[FlaskAnimationController] O GameObject do Animator está desativado! " +
                               "Mantenha o painel sempre ativo e use SolutionPanelAnimator para controlar " +
                               "a visibilidade via escala (0.001 → 1.0).");
                return;
            }

            // Aplica a animação baseado no modo selecionado
            if (animationMode == AnimationMode.UseCrossFade)
            {
                // Modo CrossFade: vai diretamente para o estado pelo nome
                PlayAnimationDirectly(outcome);
            }
            else
            {
                // Modo Parâmetros: define os parâmetros e deixa as transições do AnyState funcionarem
                ApplyAnimationParameters(outcome);
            }
        }

        /// <summary>
        /// Aplica os parâmetros de animação no Animator.
        /// </summary>
        private void ApplyAnimationParameters(SolubilityOutcome outcome)
        {
            // Converte as chaves do outcome para os valores inteiros do Animator
            int flaskTypeValue = FlaskTypeToInt(outcome.FlaskType);
            int mixtureTypeValue = MixtureTypeToInt(outcome.MixtureType);
            int solubilityValue = SolubilityResultToInt(outcome.SolubilityResult);
            int litmusValue = LitmusResultToInt(outcome.LitmusResult);

            // Define os parâmetros no Animator
            flaskAnimator.SetInteger(flaskTypeParam, flaskTypeValue);
            flaskAnimator.SetInteger(mixtureTypeParam, mixtureTypeValue);
            flaskAnimator.SetInteger(solubilityResultParam, solubilityValue);
            flaskAnimator.SetInteger(litmusResultParam, litmusValue);

            // Força o Animator a re-avaliar as transições imediatamente
            // Isso é necessário porque as transições do AnyState podem não disparar
            // automaticamente quando os parâmetros são alterados
            flaskAnimator.Update(0f);

            if (logAnimationKeys)
            {
                string animationKey = BuildAnimationKey(outcome);
                string currentState = GetCurrentStateName();
                Debug.Log($"[FlaskAnimationController] Parâmetros configurados: " +
                          $"flask={flaskTypeValue}, mixture={mixtureTypeValue}, " +
                          $"solubility={solubilityValue}, litmus={litmusValue} " +
                          $"→ Chave esperada: {animationKey} | Estado atual: {currentState}");
            }
        }

        /// <summary>
        /// Obtém o nome do estado atual do Animator para debug.
        /// </summary>
        private string GetCurrentStateName()
        {
            if (flaskAnimator == null || !flaskAnimator.isActiveAndEnabled)
                return "N/A";

            AnimatorStateInfo stateInfo = flaskAnimator.GetCurrentAnimatorStateInfo(0);

            // Tenta encontrar o nome do estado verificando os hashes conhecidos
            // Como o Unity não expõe o nome diretamente, verificamos os estados possíveis
            return $"Hash:{stateInfo.fullPathHash}, NormalizedTime:{stateInfo.normalizedTime:F2}";
        }

        /// <summary>
        /// Constrói a chave/nome da animação no formato usado nos arquivos .anim
        /// Ex: "F1+LL+Soluble+None"
        /// </summary>
        public string BuildAnimationKey(SolubilityOutcome outcome)
        {
            string flask = FlaskTypeToString(outcome.FlaskType);
            string mixture = MixtureTypeToString(outcome.MixtureType);
            string solubility = SolubilityResultToString(outcome.SolubilityResult);
            string litmus = LitmusResultToString(outcome.LitmusResult);

            return $"{flask}+{mixture}+{solubility}+{litmus}";
        }

        #region Mapeamento de Enums para Inteiros (Animator Parameters)

        private int FlaskTypeToInt(FlaskType flaskType)
        {
            return flaskType switch
            {
                FlaskType.FLASK_01 => 0,
                FlaskType.FLASK_02 => 1,
                FlaskType.FLASK_03 => 2,
                FlaskType.FLASK_04 => 3,
                _ => 0
            };
        }

        private int MixtureTypeToInt(MixtureType mixtureType)
        {
            return mixtureType switch
            {
                MixtureType.LL => 0,
                MixtureType.SL => 1,
                _ => 0
            };
        }

        private int SolubilityResultToInt(SolubilityResultKind result)
        {
            return result switch
            {
                SolubilityResultKind.Soluble => 0,
                SolubilityResultKind.InsolubleFloat => 1,
                SolubilityResultKind.InsolubleSink => 2,
                _ => 0
            };
        }

        private int LitmusResultToInt(LitmusResultKind result)
        {
            return result switch
            {
                LitmusResultKind.None => 0,
                LitmusResultKind.Basic => 1,
                LitmusResultKind.Neutral => 2,
                LitmusResultKind.Acidic => 3,
                _ => 0
            };
        }

        #endregion

        #region Mapeamento de Enums para Strings (Nome das Animações)

        private string FlaskTypeToString(FlaskType flaskType)
        {
            return flaskType switch
            {
                FlaskType.FLASK_01 => "F1",
                FlaskType.FLASK_02 => "F2",
                FlaskType.FLASK_03 => "F3",
                FlaskType.FLASK_04 => "F4",
                _ => "F1"
            };
        }

        private string MixtureTypeToString(MixtureType mixtureType)
        {
            return mixtureType switch
            {
                MixtureType.LL => "LL",
                MixtureType.SL => "SL",
                _ => "LL"
            };
        }

        private string SolubilityResultToString(SolubilityResultKind result)
        {
            return result switch
            {
                SolubilityResultKind.Soluble => "Soluble",
                SolubilityResultKind.InsolubleFloat => "Float",
                SolubilityResultKind.InsolubleSink => "Sink",
                _ => "Soluble"
            };
        }

        private string LitmusResultToString(LitmusResultKind result)
        {
            return result switch
            {
                LitmusResultKind.None => "None",
                LitmusResultKind.Basic => "Basic",
                LitmusResultKind.Neutral => "Neutral",
                LitmusResultKind.Acidic => "Acidic",
                _ => "None"
            };
        }

        #endregion

        #region Métodos de Conveniência para Testes

        /// <summary>
        /// Permite testar a animação diretamente pelo Inspector (Editor only).
        /// </summary>
        [ContextMenu("Test Animation F1+LL+Soluble+None")]
        private void TestDefaultAnimation()
        {
            if (flaskAnimator == null)
            {
                Debug.LogError("Animator não atribuído.");
                return;
            }

            flaskAnimator.SetInteger(flaskTypeParam, 0);
            flaskAnimator.SetInteger(mixtureTypeParam, 0);
            flaskAnimator.SetInteger(solubilityResultParam, 0);
            flaskAnimator.SetInteger(litmusResultParam, 0);

            // Força a transição
            flaskAnimator.Update(0f);

            Debug.Log("[FlaskAnimationController] Teste: F1+LL+Soluble+None (parâmetros definidos)");
        }

        /// <summary>
        /// Testa a animação usando CrossFade diretamente com o nome do estado.
        /// Use isso se as transições do AnyState não estiverem funcionando.
        /// </summary>
        [ContextMenu("Test CrossFade FLASK_01+LIQUID_LIQUID+Soluble+None")]
        private void TestCrossFadeAnimation()
        {
            if (flaskAnimator == null)
            {
                Debug.LogError("Animator não atribuído.");
                return;
            }

            // Nome do estado no Animator Controller
            string stateName = "FLASK_01+LIQUID_LIQUID+Soluble+None";
            flaskAnimator.CrossFade(stateName, 0f);

            Debug.Log($"[FlaskAnimationController] CrossFade para estado: {stateName}");
        }

        #endregion

        #region Método Alternativo - CrossFade Direto

        /// <summary>
        /// Método alternativo que usa CrossFade diretamente com o nome do estado.
        /// Use isso se as transições do AnyState não funcionarem.
        /// Inclui sistema de fallback para estados não encontrados.
        /// </summary>
        /// <param name="outcome">Resultado da solubilidade</param>
        public void PlayAnimationDirectly(SolubilityOutcome outcome)
        {
            if (outcome == null || flaskAnimator == null)
            {
                Debug.LogError("[FlaskAnimationController] Outcome ou Animator é nulo.");
                return;
            }

            string stateName = BuildStateNameForAnimator(outcome);
            string finalStateName = FindValidStateWithFallback(outcome, stateName);

            if (string.IsNullOrEmpty(finalStateName))
            {
                Debug.LogError($"[FlaskAnimationController] Nenhum estado válido encontrado para: '{stateName}'");
                Debug.LogError("[FlaskAnimationController] Adicione a animação correspondente ao Animator Controller.");
                return;
            }

            flaskAnimator.CrossFade(finalStateName, 0f);

            if (logAnimationKeys)
            {
                if (finalStateName != stateName)
                {
                    // Usa Log ao invés de LogWarning para evitar stack trace desnecessário
                    Debug.Log($"[FlaskAnimationController] ⚠️ Estado '{stateName}' não encontrado. Usando fallback: '{finalStateName}'");
                }
                else
                {
                    Debug.Log($"[FlaskAnimationController] ✓ CrossFade para estado: '{finalStateName}'");
                }

                // Log do estado atual após um frame
                StartCoroutine(LogCurrentStateAfterFrame());
            }
        }

        /// <summary>
        /// Tenta encontrar um estado válido, usando fallbacks se necessário.
        /// Ordem de fallback:
        /// 1. Estado exato
        /// 2. Mesmo resultado (mixture+solubility+litmus) com outro frasco (apenas altera o FLASK)
        /// 3. Estado padrão absoluto: Empty_flask
        /// </summary>
        private string FindValidStateWithFallback(SolubilityOutcome outcome, string originalStateName)
        {
            // 1. Tenta o estado original
            if (StateExists(originalStateName))
                return originalStateName;

            string mixture = outcome.MixtureType == MixtureType.LL ? "LIQUID_LIQUID" : "SOLID_LIQUID";
            string flask = outcome.FlaskType.ToString();
            string solubility = outcome.SolubilityResult.ToString();
            string litmus = outcome.LitmusResult.ToString();

            // Uma lista pra manipular dinamicamente a alternância de frascos
            List<string> flaskList = new List<string>
            {
                "FLASK_01",
                "FLASK_02",
                "FLASK_03",
                "FLASK_04"
            };

            flaskList.Remove(flask);

            Stack<string> flaskStack = new Stack<string>(flaskList);

            // 2. Tenta manter os mesmos parâmetros (mixture, solubility, litmus) mas com outros FLASK
            while (flaskStack.Count > 0)
            {
                string fallbackWithOtherFlask = $"{flaskStack.Pop()}+{mixture}+{solubility}+{litmus}";

                // TODO: Hotfix temporário. Achar abordagem melhor
                if(litmus == "Neutral")
                    litmus = "None"; // Nenhuma animação foi pensada para o resultado neutro, então vamos usar a animação sem litmus como fallback

                if (StateExists(fallbackWithOtherFlask))
                    return fallbackWithOtherFlask;
            }

            // 3. Estado padrão absoluto
            const string defaultState = "Empty_flask";
            if (StateExists(defaultState))
                return defaultState;

            return null;
        }

        /// <summary>
        /// Verifica se um estado existe no Animator Controller.
        /// </summary>
        private bool StateExists(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            return flaskAnimator.HasState(0, hash);
        }

        private System.Collections.IEnumerator LogCurrentStateAfterFrame()
        {
            yield return null; // Espera 1 frame
            if (flaskAnimator != null && flaskAnimator.isActiveAndEnabled)
            {
                var stateInfo = flaskAnimator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"[FlaskAnimationController] Estado após CrossFade - Hash: {stateInfo.fullPathHash}, " +
                          $"IsPlaying: {!stateInfo.IsName("Empty")}, Length: {stateInfo.length:F2}s");
            }
        }

        /// <summary>
        /// Constrói o nome do estado no formato usado pelo Animator Controller.
        /// Ex: "FLASK_01+LIQUID_LIQUID+Soluble+None"
        /// </summary>
        private string BuildStateNameForAnimator(SolubilityOutcome outcome)
        {
            string flask = outcome.FlaskType.ToString(); // FLASK_01, FLASK_02, etc.
            string mixture = outcome.MixtureType == MixtureType.LL ? "LIQUID_LIQUID" : "SOLID_LIQUID";
            string solubility = outcome.SolubilityResult.ToString(); // Soluble, InsolubleFloat, InsolubleSink
            string litmus = outcome.LitmusResult.ToString(); // None, Basic, Neutral, Acidic

            return $"{flask}+{mixture}+{solubility}+{litmus}";
        }

        #endregion
    }
}
