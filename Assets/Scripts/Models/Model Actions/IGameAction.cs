using System.Collections;
using System.Threading.Tasks;

public interface IGameAction
{
    int Cost { get; } // 1 = short, 2 = long
    bool CanExecute(GameActionContext ctx);
    IEnumerator Execute(GameActionContext ctx); // or IEnumerator for coroutines
}
