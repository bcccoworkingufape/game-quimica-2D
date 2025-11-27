namespace Domain
{
    /// <summary>
    /// Resultado de solubilidade (tabela: S, I↑, I↓).
    /// </summary>
    public enum SolubilityResultKind
    {
        Soluble,         // S
        InsolubleFloat,  // I↑ (fase menos densa)
        InsolubleSink    // I↓ (fase mais densa)
    }
}
