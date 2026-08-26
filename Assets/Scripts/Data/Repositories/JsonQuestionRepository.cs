using System.Collections.Generic;
using Newtonsoft.Json;
using Domain;

namespace Data
{
    /// <summary>
    /// Repositorio JSON de questões. De <see cref="DataIndex.QuestionsFile"/>
    /// via <see cref="IJsonProvider"/>.
    /// </summary>
    public class JsonQuestionRepository : IQuestionRepository
    {
        private readonly Dictionary<int, Question> _byId;
        private readonly List<Question> _ordered;

        public JsonQuestionRepository(IJsonProvider provider)
        {
            var json = provider.LoadText(DataIndex.QuestionsFile);

            // Question nao precisava de DTO: as chaves do JSON ja correspondem, uma a
            // uma, aos parametros do construtor da entidade, e o Newtonsoft materializa
            // por esse construtor — o que preserva as invariantes de Question.cs:33-46.
            // A entidade segue sem atributo de serializacao, sem setter e com um unico
            // construtor publico. Ver SpikeQuestionDeserialization no projeto de testes.
            var questions = JsonConvert.DeserializeObject<List<Question>>(json) ?? new List<Question>();

            _byId = new Dictionary<int, Question>(questions.Count);
            _ordered = new List<Question>(questions.Count);

            foreach (var q in questions)
            {
                _byId[q.Id] = q;
                _ordered.Add(q);
            }
        }

        public IReadOnlyList<Question> ListAll() => _ordered;

        public Question GetById(int id) =>
            _byId.TryGetValue(id, out var q) ? q : null;
    }
}
