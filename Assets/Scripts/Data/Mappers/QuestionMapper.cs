using Domain;

namespace Data
{
    /// <summary>Conversores entre <see cref="QuestionDto"/> e <see cref="Question"/>.</summary>
    public static class QuestionMapper
    {
        public static Question ToDomain(this QuestionDto dto)
        {
            return new Question(
                dto.id,
                dto.compoundId,
                dto.description,
                dto.alternatives,
                dto.correctAnswer,
                dto.hint,
                dto.feedback);
        }
    }
}
