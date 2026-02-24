namespace Monk.Infrastructure
{
    public class SaveManager
    {
        public void Save(string key, object data)
        {
        }

        public T Load<T>(string key)
        {
            return default;
        }

        public bool HasSave(string key)
        {
            return false;
        }

        public void DeleteSave(string key)
        {
        }
    }
}
