namespace Star {

  public struct XY {
    public int x, y;

    public XY(int x, int y) {
      this.x = x;
      this.y = y;
    }
  }

  public class IterableRect : IEnumerable<XY> {

    private StarIntRect rect;

    public IEnumerator<XY> GetEnumerator() {
      for (int y = rect.y0; y <= rect.y1; ++y) {
        for (int x  = rect.x0; x <= rect.x1; ++x) {
          yield return new XY(x,y);
        }
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public IterableRect(StarIntRect rect) {
      this.rect = rect;
    }

    public IterableRect(int x0, int y0, int w, int h) {
      this.rect = new StarIntRect(x0, y0, w, h);
    }


  }

  public struct StarIntRect {
    public int x0, y0, w, h = -1;

    public StarIntRect(int x0, int y0, int w, int h) {
      this.x0 = x0;
      this.y0 = y0;
      this.w = w;
      this.h = h;
    }

    public int x1 => (x0 + w) - 1;
    public int y1 => (y0 + h) - 1;

    public bool IterateOver(ref int x, ref int y) {
      if (x < x1) { ++x; return true;}
      if (y < y1) { x=0; ++y; return true;}
      return false;
    }

    public bool Exists() => w > 0 && h > 0;

    public bool ContainsPoint(int x, int y) => Exists() && x >= x0 && y >= y0 && x <= x1 && y <= y1;
    //public bool ContainsPoint(uint x, uint y) => Exists() && x >= x0 && y >= y0 && x <= x1 && y <= y1;

    public bool ContainsPoint(XY xy) => ContainsPoint(xy.x, xy.y);

    public override string ToString() => $"StarIntRect: ({x0}, {y0}), W: {w}, H: {h}";

    public static StarIntRect Intersection(StarIntRect a, StarIntRect b) {
      if (!a.Exists() || !b.Exists() || a.x1 < b.x0 || b.x1 < a.x0 || a.y1 < b.y0 || b.y1 < a.y0) {
        return new StarIntRect(-1,-1,-1,-1);      //An empty star int rect.
      }

      int x0 = Math.Max(a.x0, b.x0);
      int x1 = Math.Min(a.x1, b.x1);
      int y0 = Math.Max(a.y0, b.y0);
      int y1 = Math.Min(a.y1, b.y1);

      int w = x1 - x0 + 1;
      int h = y1 - y0 + 1;

      return new StarIntRect(x0,y0,w,h);
    }



  }

  static class Util {

    //Swaps a and b
    public static void Swap<T>(ref T a, ref T b) {
      T temp = a;
      a = b;
      b = temp;
    }

    //This was adapted from code written by Jay Byford-Rew on Stackoverflow
    //https://stackoverflow.com/questions/6219454/efficient-way-to-remove-all-whitespace-from-string/
    public static string RemoveSpaces(string str) {
      if (str.IndexOf(' ') == -1) { return str; }

      //str = string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
      str = string.Join("", str.Split(' ', StringSplitOptions.RemoveEmptyEntries));
      return str;
    }
    
  }
}