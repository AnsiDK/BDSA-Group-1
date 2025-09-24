namespace SimpleDB;
public interface IDatabaseRepository<T>
{

    IEnumerable<T> Read(int limit);
    IEnumerable<T> ReadAll();
    void Store(T record);
}
