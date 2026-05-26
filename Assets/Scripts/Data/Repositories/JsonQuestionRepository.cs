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
            var dtos = JsonConvert.DeserializeObject<List<QuestionDto>>(json) ?? new List<QuestionDto>();

            _byId = new Dictionary<int, Question>(dtos.Count);
            _ordered = new List<Question>(dtos.Count);

            foreach (var dto in dtos)
            {
                var q = dto.ToDomain();
                _byId[q.Id] = q;
                _ordered.Add(q);
            }
        }

        public IReadOnlyList<Question> ListAll() => _ordered;

        public Question GetById(int id) =>
            _byId.TryGetValue(id, out var q) ? q : null;
    }
}
