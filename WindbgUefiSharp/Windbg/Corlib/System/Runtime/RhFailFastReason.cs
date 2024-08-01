namespace System.Runtime
{
    public enum RhFailFastReason
    {
        Unknown,
        InternalError,
        UnhandledException_ExceptionDispatchNotAllowed,
        UnhandledException_CallerDidNotHandle,
        ClassLibDidNotTranslateExceptionID,
        UnhandledException,
        UnhandledExceptionFromPInvoke
    }
}
