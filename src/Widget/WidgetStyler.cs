namespace Star.Widget {
  public class WidgetStyler {

    public SFML.Graphics.Color? fg;
    public SFML.Graphics.Color? bg;

    public static SFML.Graphics.Color defaultFG = SFML.Graphics.Color.White;
    public static SFML.Graphics.Color? defaultBG = null;

    //This generates a new WidgetStyler with any base properties from the caller.
    //i.e. a.Compound(b) returns a new value c which is identical to a but with values overwritten
    //by non-null values in b
    public WidgetStyler Compound(WidgetStyler? other) {
      WidgetStyler c = new WidgetStyler() {fg=this.fg, bg=this.bg};
      if (other == null) return c;
      c.fg = other.fg ?? c.fg;
      c.bg = other.bg ?? c.bg;
      return c;
    }

    
    
  }
}