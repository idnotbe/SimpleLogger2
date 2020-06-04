namespace SimpleLogger2
{
    public enum CallerInfo
    {
        SourceLine,
        ClassMethod, // 퍼포먼스 떨어짐
        NoCallerInfo
    }
}