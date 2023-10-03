namespace Framework.Shared.Extensions
{
    public static class InterfaceExistenceChecker
    {
        public static bool Check<T>(Type interfaceType) where T : class
            => typeof(T).GetInterface($"{interfaceType.FullName}", true) != null;
    }
}
