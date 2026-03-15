using System.Collections;
using System.Threading.Tasks;

public interface IModelAction
{
    int Cost { get; } // 1 = short, 2 = long
    bool CanExecute(ModelActionContext ctx);
    IEnumerator Execute(ModelActionContext ctx); // or IEnumerator for coroutines
}
