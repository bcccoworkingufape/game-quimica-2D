namespace Domain
{
    /// <summary>
    /// Resultado do tornassol (Inc, A, V).
    /// </summary>
    public enum LitmusResultKind
    {
        None,    // Inc - não há mudança perceptível
        Neutral, // permanece incolor / neutro
        Acidic,  // fica vermelho
        Basic    // fica azul
    }
}
