namespace Saltarelle.Compiler {
    public interface IErrorReporter {
        void Error(string message);
        void Warning(string message);
    }
}