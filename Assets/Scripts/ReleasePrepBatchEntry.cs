#if UNITY_EDITOR
public static class ReleasePrepBatchEntry
{
    public static void Run()
    {
        CoffeeKing.EditorTools.ReleasePrepAudit.RunFromBatchMode();
    }
}
#endif
