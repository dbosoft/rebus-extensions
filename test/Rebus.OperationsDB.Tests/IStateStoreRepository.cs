using Ardalis.Specification;

namespace Dbosoft.Rebus.OperationsDB.Tests;

public interface IStateStoreRepository<T> : IRepositoryBase<T> where T: class
{

}