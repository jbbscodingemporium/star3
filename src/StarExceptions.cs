namespace Star {
  public class StarExcept : Exception {
    public StarExcept(string message) : base(message) {
      Log.Write(message);
    }

    public StarExcept(string message, Exception e) : base(message, e) {
      Log.Write(message);
    }
  }
}