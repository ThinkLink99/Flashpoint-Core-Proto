using System.Threading.Tasks;

public interface IUnitAction
{
    int Cost { get; } // 1 = short, 2 = long
    bool CanExecute(Model unit);
    Task Execute(Model unit); // or IEnumerator for coroutines
}
