namespace _06ImageRecoloring.Pipeline
{
    public sealed class ProcessingResult
    {
        public ProcessingResult (bool success, string message, int exitCode)
        {
            Success = success;
            Message = message;
            ExitCode = exitCode;
        }

        public bool Success { get; }
        public string Message { get; }
        public int ExitCode { get; }
    }
}
