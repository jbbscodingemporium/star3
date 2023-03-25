namespace Star.Widget {

  public class Sylph {

    //Depth is how deep it is in the tree. Not sure if I need this, really!
    public int Depth = 0;

    //Priority is from lowest to highest.
    public int Priority = 0;

    public string name = "";


    //The xys that this covers.
    //These can be deleted by the Widget!
    protected List<XY> xys = new List<XY>();

    public void ClearXYs() {
      xys = new List<XY>();
    }

    public void RemoveAnyPointsOutside(StarIntRect rect) {
      for (int i = 0; i < xys.Count; ++i) {
        if (rect.ContainsPoint(xys[i])) continue;
        xys.RemoveAt(i);
        --i;
      }
    }

    //public void AddXY(int x, int y, bool isNotWhitespace) {
      public void AddXY(int x, int y) {
      xys.Add(new XY(x,y));
    }

    public string XYsToString() {
      string str = "";
      foreach (var xy in xys) {
        str += $"({xy.x},{xy.y}), ";
      }
      return str;
    }

    public Sylph(string name, int depth) {
      this.name = name;
      this.Depth = depth;
    }

    public virtual void Update(TextWidget.TextCellData data) {
      SFML.Graphics.Color color = SFML.Graphics.Color.Blue;
      if (Depth == 1) { color = SFML.Graphics.Color.Green; }
      foreach (var xy in xys) {
        data.SetOverrideFG(xy.x, xy.y, color);
      }
    }

    public void UpdateOld(TextWidget.TextCellData data) {
      foreach (var xy in xys) {
        data.SetOverrideCharacter(xy.x, xy.y, 'z');
      }

      var ir = new IterableRect(0,0,data.W,data.H);
      foreach (var xy in ir) {
        char character = data.GetCharacter(xy.x,xy.y);
        char? overrideChar = null;
        List<char> vowels = new List<Char>() { 'a','e','i','o','u'}; 
        if ( vowels.Contains(character)) {
          overrideChar = '*';
          data.SetOverrideCharacter(xy.x,xy.y,overrideChar);
        }
      }

    }


  }

  public static class SylphFactory {
    public static Sylph Generate(string name, int depth, Dictionary<string,string>? passedTags) {

      var tags = passedTags ?? new Dictionary<string, string>();

      Sylph? attempt = ColorSylph.GenerateFromString(name, depth, tags);
      attempt = attempt ?? RandomCapsSylph.GenerateFromString(name, depth, tags);
      attempt = attempt ?? LeetSylph.GenerateFromString(name,depth,tags);

      if (attempt is not null) { return attempt; }

      return new Sylph(name, depth);
    }
  }

}