namespace Domain
{
    public interface IScoringService
    {
        int ComputePoints(bool isCorrect, int streak);
    }
}
