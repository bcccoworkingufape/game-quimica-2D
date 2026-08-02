using System.Collections.Generic;
using Domain;

namespace Data
{
    /// <summary>Repositorio de questões do jogo (carregadas de JSON em Resources).</summary>
    public interface IQuestionRepository
    {
        IReadOnlyList<Question> ListAll();
        Question GetById(int id);
    }
}
