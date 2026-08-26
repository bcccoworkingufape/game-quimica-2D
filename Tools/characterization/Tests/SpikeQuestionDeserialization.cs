using System.Collections.Generic;
using System.Linq;
using Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Molz.Characterization
{
    /// <summary>
    /// SPIKE da Etapa B1 — prova (ou refuta) que o Newtonsoft materializa
    /// Domain.Question diretamente do JSON, SEM contaminar o Domain.
    ///
    /// Question tem 7 propriedades get-only, nenhum setter, nenhum construtor
    /// sem parametros e invariantes no construtor (Question.cs:33-46).
    /// A questao nao e "da pra remover o DTO", e sim "da pra remover o DTO
    /// preservando as invariantes e sem colocar serializacao dentro da entidade".
    ///
    /// Criterio de aceite, todos obrigatorios:
    ///   1. o construtor da entidade e efetivamente invocado;
    ///   2. politica de null identica (null -> string.Empty);
    ///   3. alternativas com mesma contagem, mesmo conteudo e MESMA ORDEM;
    ///   4. lista vazia/ausente -> [CorrectAnswer];
    ///   5. zero atributos de serializacao em Domain;
    ///   6. resultado identico, campo a campo, ao JSON de origem.
    ///
    /// Ate a Etapa B1 o criterio 6 comparava contra QuestionDto + QuestionMapper.
    /// Removidos os dois, a suite permanece como regressao: e ela que trava o
    /// acoplamento implicito entre as chaves do JSON e os nomes dos parametros
    /// do construtor de Question, que agora e o que sustenta a materializacao.
    /// </summary>
    [TestFixture]
    public class SpikeQuestionDeserialization
    {
        private static string Json() => Dataset.RawJson("QuestionsData.json");

        private static List<Question> Direto() =>
            JsonConvert.DeserializeObject<List<Question>>(Json());

        // ------------------------------------------------ criterio 6
        [Test]
        public void Spike_DirectDeserialization_MatchesRawJson_FieldByField()
        {
            // Ate a Etapa B1 esta comparacao era feita contra o caminho
            // QuestionDto + QuestionMapper. Removidos os dois, o contraste passa
            // a ser com uma leitura independente do JSON cru via JObject, que
            // nao compartilha codigo com a materializacao sob teste.
            var cru = JArray.Parse(Json());
            var direto = Direto();

            Assert.AreEqual(cru.Count, direto.Count, "mesma cardinalidade");

            for (var i = 0; i < cru.Count; i++)
            {
                var e = cru[i];
                var q = direto[i];
                var ctx = "indice " + i + " (id " + (int)e["id"] + ")";

                Assert.AreEqual((int)e["id"], q.Id, ctx + " Id");
                Assert.AreEqual((int)e["compoundId"], q.CompoundId, ctx + " CompoundId");
                Assert.AreEqual((string)e["description"], q.Description, ctx + " Description");
                Assert.AreEqual((string)e["correctAnswer"], q.CorrectAnswer, ctx + " CorrectAnswer");
                Assert.AreEqual((string)e["hint"], q.Hint, ctx + " Hint");
                Assert.AreEqual((string)e["feedback"], q.Feedback, ctx + " Feedback");

                // criterio 3: contagem, conteudo E ordem
                CollectionAssert.AreEqual(
                    e["alternatives"].Select(x => (string)x).ToList(),
                    q.Alternatives.ToList(),
                    ctx + " Alternatives (conteudo e ordem)");
            }
        }

        [Test]
        public void Spike_DirectDeserialization_DoesNotDuplicateAlternatives()
        {
            // Risco concreto: ObjectCreationHandling.Auto do Newtonsoft pode
            // POPULAR a colecao ja criada pelo construtor em vez de substitui-la,
            // o que dobraria as alternativas em silencio.
            var direto = Direto();

            Assert.IsTrue(direto.All(q => q.Alternatives.Count == 4),
                "todas as 10 questoes tem exatamente 4 alternativas; " +
                "contagem diferente indica populacao dupla: " +
                string.Join(", ", direto.Select(q => q.Id + ":" + q.Alternatives.Count)));
        }

        // ------------------------------------------------ criterios 1, 2 e 4
        [Test]
        public void Spike_DirectDeserialization_RunsEntityInvariants_NullsBecomeEmpty()
        {
            const string json = "[{\"id\":1,\"compoundId\":2}]";

            var q = JsonConvert.DeserializeObject<List<Question>>(json).Single();

            Assert.AreEqual(1, q.Id);
            Assert.AreEqual(2, q.CompoundId);
            Assert.AreEqual(string.Empty, q.Description, "invariante de Question.cs:33");
            Assert.AreEqual(string.Empty, q.CorrectAnswer, "invariante de Question.cs:34");
            Assert.AreEqual(string.Empty, q.Hint, "invariante de Question.cs:35");
            Assert.AreEqual(string.Empty, q.Feedback, "invariante de Question.cs:36");
        }

        [Test]
        public void Spike_DirectDeserialization_EmptyAlternatives_FallsBackToCorrectAnswer()
        {
            const string json =
                "[{\"id\":1,\"compoundId\":1,\"correctAnswer\":\"Resposta\",\"alternatives\":[]}]";

            var q = JsonConvert.DeserializeObject<List<Question>>(json).Single();

            Assert.AreEqual(1, q.Alternatives.Count, "invariante de Question.cs:39-42");
            Assert.AreEqual("Resposta", q.Alternatives[0]);
        }

        [Test]
        public void Spike_DirectDeserialization_ExplicitNullAlternatives_FallsBack()
        {
            const string json =
                "[{\"id\":1,\"compoundId\":1,\"correctAnswer\":\"R\",\"alternatives\":null}]";

            var q = JsonConvert.DeserializeObject<List<Question>>(json).Single();

            Assert.AreEqual(1, q.Alternatives.Count);
            Assert.AreEqual("R", q.Alternatives[0]);
        }

        [Test]
        public void Spike_DirectDeserialization_PreservesAlternativeOrderExactly()
        {
            const string json =
                "[{\"id\":1,\"compoundId\":1,\"correctAnswer\":\"B\"," +
                "\"alternatives\":[\"Z\",\"A\",\"M\",\"B\"]}]";

            var q = JsonConvert.DeserializeObject<List<Question>>(json).Single();

            CollectionAssert.AreEqual(new[] { "Z", "A", "M", "B" }, q.Alternatives.ToList(),
                "a ordem do JSON e a ordem exibida no painel; nao pode ser reordenada");
        }

        [Test]
        public void Spike_DirectDeserialization_IgnoresUnknownJsonFields()
        {
            // O JSON pode ganhar campos novos sem que a entidade os conheca,
            // exatamente como acontece hoje com mixtureType em SolubilityData.json.
            const string json =
                "[{\"id\":9,\"compoundId\":9,\"correctAnswer\":\"X\"," +
                "\"alternatives\":[\"X\"],\"campoNovo\":123}]";

            var q = JsonConvert.DeserializeObject<List<Question>>(json).Single();

            Assert.AreEqual(9, q.Id);
            Assert.AreEqual("X", q.CorrectAnswer);
        }

        // ------------------------------------------------ criterio 5
        [Test]
        public void Spike_DomainEntity_HasNoSerializationContamination()
        {
            var t = typeof(Question);

            // nenhum atributo de serializacao no tipo nem nos membros
            var atributos = t.GetCustomAttributes(true)
                .Concat(t.GetProperties().SelectMany(p => p.GetCustomAttributes(true)))
                .Concat(t.GetConstructors().SelectMany(c => c.GetCustomAttributes(true)))
                .Select(a => a.GetType().FullName)
                .ToList();

            Assert.IsFalse(atributos.Any(n => n.StartsWith("Newtonsoft.")),
                "Domain.Question nao pode carregar atributo do Newtonsoft: " +
                string.Join(", ", atributos));

            // nenhuma propriedade com setter publico
            Assert.IsFalse(t.GetProperties().Any(p => p.CanWrite),
                "Domain.Question nao pode ganhar setter para servir ao serializer");

            // um unico construtor publico, o de dominio, com 7 parametros
            var ctors = t.GetConstructors();
            Assert.AreEqual(1, ctors.Length, "nao pode existir construtor extra para o serializer");
            Assert.AreEqual(7, ctors[0].GetParameters().Length);
        }

        [Test]
        public void Spike_DomainAssembly_DoesNotReferenceNewtonsoft()
        {
            // Vale para o tipo: nenhum membro de Question expoe tipo do Newtonsoft.
            var t = typeof(Question);
            var tiposExpostos = t.GetProperties().Select(p => p.PropertyType)
                .Concat(t.GetConstructors().SelectMany(c => c.GetParameters().Select(p => p.ParameterType)))
                .Select(x => x.Namespace ?? "")
                .Distinct()
                .ToList();

            Assert.IsFalse(tiposExpostos.Any(ns => ns.StartsWith("Newtonsoft")),
                "a superficie de Question nao pode citar tipos do Newtonsoft: " +
                string.Join(", ", tiposExpostos));
        }
    }
}
