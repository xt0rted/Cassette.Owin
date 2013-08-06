using System.IO.IsolatedStorage;

using Cassette.Utilities;

namespace Cassette.Owin
{
    public static class IsolatedStorageContainer
    {
        // ReSharper disable ConvertClosureToMethodGroup - http://stackoverflow.com/q/9113791/39605
        public static readonly DisposableLazy<IsolatedStorageFile> LazyStorage = new DisposableLazy<IsolatedStorageFile>(() => CreateIsolatedStorage());
        // ReSharper restore ConvertClosureToMethodGroup

        public static IsolatedStorageFile IsolatedStorageFile
        {
            get { return LazyStorage.Value; }
        }

        public static IsolatedStorageFile CreateIsolatedStorage()
        {
            return IsolatedStorageFile.GetMachineStoreForAssembly();
        }

        public static void Dispose()
        {
            LazyStorage.Dispose();
        }
    }
}