using SFML.Graphics;

namespace Star {

  //All sprites in spritesheets are numbered as follows:
  // 0  1  2  3  4  5  6  7  8  9
  //10 11 12 13 14 15 16 17 18 19
  //20 21 22 23 24 25 26 etc
  public class SpriteSheet {
    public Texture tex { get; private set; }

    private int lastSpriteIndex;
    private int rows;
    private int columns;

    public int tileW {get; private set;}
    public int tileH {get; private set;}

    public IntRect GetSpriteBounds(int index) {
      if (index < 0 || index > lastSpriteIndex || index > rows*columns - 1) {
        throw new StarExcept($"Error: Tried to grab bounds on spritesheet with an index of {index}");
      }
      int x = index % columns;
      int y = index / columns;

      return new IntRect(x*tileW, y*tileH, tileW, tileH);
    }

    //Last sprite index is an optional parameter that indicates the largest allowed index. 
    //If it's -1 then assume every index within rows/columns range is accessible.
    public SpriteSheet(Texture tex, int rows, int columns, int tileW, int tileH, int lastSpriteIndex=-1) {
      this.tex = tex;
      
      this.rows = rows;
      this.columns = columns;

      this.lastSpriteIndex = lastSpriteIndex < 0 ? rows*columns - 1 : lastSpriteIndex;

      this.tileW = tileW;
      this.tileH = tileH;
    }
  }

  public class ASCIISheet : SpriteSheet {
    public ASCIISheet(Texture tex, int tileW, int tileH) : base(tex, 16, 16, tileW, tileH, -1) {}

    public const char PIPESW = (char)191;
    public const char PIPENE = (char)192;
    public const char PIPENW = (char)217;
    public const char PIPESE = (char)218;

    public const char PIPENS = (char)179;
    public const char PIPEEW = (char)196;
    public const char PIPENSEW = (char)197;
    public const char SQUARE = (char)254;
    

    public IntRect GetSpriteBounds(char character) => GetSpriteBounds((int)character);
  }

  public static class AllSpriteSheets {

    //public static SpriteSheet ascii { get; private set; }
    public static ASCIISheet ascii { get; private set; }
    public static SpriteSheet environment { get; private set; }

    static AllSpriteSheets() {
      TextureLibrary.LoadIfNotLoaded();   //Guarantees that the spritesheets are loaded.

      var asciiTexture = TextureLibrary.GetTexture(TEXID.ASCII);
      //ascii = new ASCIISheet(asciiTexture, 8, 8);
      ascii = new ASCIISheet(asciiTexture, 8, 8);

      var envTexture = TextureLibrary.GetTexture(TEXID.ORYX_ENV);
      environment = new SpriteSheet(envTexture, 16, 16, 8, 8);
    }
  }


}