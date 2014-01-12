using System.Reflection;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Thread safe, generic singleton.
    /// </summary>
    /// <remarks>
    /// http://www.codeproject.com/Articles/33770/Static-constructors-and-way-forward-NET-optimized
    /// </remarks>
    public class Singleton<T> where T : class
    {
        public static T Instance
        {
            get
            {
                return SingletonInternal.instance;
            }
        }

        private class SingletonInternal
        {
            // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
            static SingletonInternal()
            {
            }

            // To be able to instantiate a class without a public constructor
            internal static readonly T instance = typeof(T).InvokeMember(typeof(T).Name,
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null) as T;
        }
    }
}
