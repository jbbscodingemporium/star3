namespace Star {
  public static class Log {

    static Logger? logger;

    public static void Write(string str) {
      logger?.Write(str);
    }

    public static void SetLogger(Logger assignedLogger) {
      logger = assignedLogger;
    }
  }

  public abstract class Logger {
    public abstract void Write(String str);
  }

  public class ConsoleLogger : Logger {
    public override void Write(String str) {
      Console.WriteLine(str);
    }
  }

}