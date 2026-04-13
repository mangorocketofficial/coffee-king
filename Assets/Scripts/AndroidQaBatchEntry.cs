#if UNITY_EDITOR
public static class AndroidQaBatchEntry
{
    public static void Build()
    {
        CoffeeKing.EditorTools.AndroidQaBuildTools.BuildAndroidQaApk();
    }
}
#endif
