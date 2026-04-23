using System.Collections.Generic;

namespace Domain
{
    public interface ISolventRepository
    {
        IReadOnlyList<Solvent> ListAll();
        Solvent GetById(int id);
    }
}
