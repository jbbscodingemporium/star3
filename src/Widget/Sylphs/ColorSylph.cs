namespace Star.Widget {

  public class ColorSylph : Sylph {

    public SFML.Graphics.Color color;
    public ColorSylph(string name, int depth) : base(name, depth) {
      color = SFML.Graphics.Color.White;
    }
    public ColorSylph(string name, int depth, SFML.Graphics.Color color) : base(name, depth) {
      this.color = color;
    }

    public override void Update(TextWidget.TextCellData data) {
      foreach (var xy in xys) {
        data.SetOverrideFG(xy.x, xy.y, color);
      }
    }

    static Dictionary<string, SFML.Graphics.Color> StringsToColors = new Dictionary<string, SFML.Graphics.Color>() {
      {"red", SFML.Graphics.Color.Red },
      {"cyan", SFML.Graphics.Color.Cyan },
      {"yellow", SFML.Graphics.Color.Yellow }
    };

    //Returns null if you can't generate anything.
    public static ColorSylph? GenerateFromString(string name, int depth, Dictionary<string,string> tags) {

      if (StringsToColors.ContainsKey(name)) {
        return new ColorSylph(name, depth, StringsToColors[name]);
      }
      
      //if (name == "yellow") { return new ColorSylph(name, depth, SFML.Graphics.Color.Yellow); }

      if (name == "color") {
        SFML.Graphics.Color color = SFML.Graphics.Color.White;
        if (tags.ContainsKey("hue")) {
          string hueString = tags["hue"];
          if (StringsToColors.ContainsKey(hueString)) {
            color = StringsToColors[hueString];
          }
        }
        return new ColorSylph(name, depth, color);
      }

      return null;
    }

  }
  
}