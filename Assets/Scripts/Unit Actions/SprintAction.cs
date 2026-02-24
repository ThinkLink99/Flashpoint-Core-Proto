using System;
using System.Threading.Tasks;

public class SprintAction : IUnitAction
{
    public int Cost => 2;
    public bool CanExecute(Model unit) => throw new NotImplementedException(); 
    public Task Execute(Model unit) => throw new NotImplementedException();
}
