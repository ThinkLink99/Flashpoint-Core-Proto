using System;
using System.Collections;
using System.Threading.Tasks;

public class SprintAction : IUnitAction
{
    public int Cost => 2;
    public bool CanExecute(Model unit) => throw new NotImplementedException(); 
    public IEnumerator Execute(Model unit) => throw new NotImplementedException();
}
