namespace Foodo.Application.Abstraction.InfraRelated
{
	public interface ICacheService
	{
		void Set(string key, object value);
		T Get<T>(string key);
		void Remove(string key);
		void RemoveByPrefix(string prefix);
	}
}
