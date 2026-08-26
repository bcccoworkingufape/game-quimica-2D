using System.Collections.Generic;
using System.Linq;
using Data;
using Domain;
using Newtonsoft.Json;
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
    ///   6. resultado identico, campo a campo, ao caminho DTO + Mapper.
    /// </summary>
    [TestFixture]
    public class SpikeQuestionDeserialization
    {
        private static string Json() => Dataset.RawJson("QuestionsData.json");

        private static List<Question> ViaDto()
        {
            var dtos = JsonConvert.DeserializeObject<List<QuestionDto>>(Json());
            return dtos.Select(d => d.ToDomain()).ToList();
        }

        private static List<Question> Direto() =>
            JsonConvert.DeserializeObject<List<Question>>(Json());

        // ------------------------------------------------ criterio 6
        [Test]
        public void Spike_DirectDeserialization_MatchesDtoPath_FieldByField()
        {
            var viaDto = ViaDto();
            var direto = Direto();

            Assert.AreEqual(viaDto.Count, direto.Count, "mesma cardinalidade");

            for (var i = 0; i < viaDto.Count; i++)
            {
                var a = viaDto[i];
                var b = direto[i];
                var ctx = "indice " + i + " (id " + a.Id + ")";

                Assert.AreEqual(a.Id, b.Id, ctx + " Id");
                Assert.AreEqual(a.CompoundId, b.CompoundId, ctx + " CompoundId");
                Assert.AreEqual(a.Description, b.Description, ctx + " Description");
                Assert.AreEqual(a.CorrectAnswer, b.CorrectAnswer, ctx + " CorrectAnswer");
                Assert.AreEqual(a.Hint, b.Hint, ctx + " Hint");
                Assert.AreEqual(a.Feedback, b.Feedback, ctx + " Feedback");

                // criterio 3: contagem, conteudo E ordem
                CollectionAssert.AreEqual(a.Alternatives.ToList(), b.Alternatives.ToList(),
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
