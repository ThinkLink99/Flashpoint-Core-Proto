using System;
using System.Threading.Tasks;

public class AdvanceAction : IUnitAction
{
    public int Cost => 1;
    public bool CanExecute(Model unit) => throw new NotImplementedException();
    public Task Execute(Model unit) => throw new NotImplementedException();
}
