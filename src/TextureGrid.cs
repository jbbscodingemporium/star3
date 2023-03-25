using SFML.System;
using SFML.Graphics;

namespace Star {

  public class TileData {
    public uint tileID {get; private set;}      //The location on the spritesheet
    public Color fgColor {get; private set;} = Color.White;
    public Color bgColor {get; private set;} = Color.Transparent;

    public TileData() {}

    public TileData(TileData other) {
      this.tileID = other.tileID;
      this.fgColor = other.fgColor;
      this.bgColor = other.bgColor;
    }

    //Returns true if the fg color changed.
    public bool ChangeFGColor(Color color) {
      bool changed = (color != this.fgColor);
      this.fgColor = color;
      return changed;
    }

    //Returns true if the bg color changed.
    public bool ChangeBGColor(Color color) {
      bool changed = (color != this.bgColor);
      this.bgColor = color;
      return changed;
    }

    //Returns true if the tile changed.
    public bool ChangeTile(uint newTile) {
      bool changed = (newTile != this.tileID);
      this.tileID = newTile;
      return changed;
    }





  }

  //A tile map of ascii characters
  public class ASCIIGrid : TextureGrid {
    public ASCIIGrid(int w, int h) : base(AllSpriteSheets.ascii, w, h) {}

    public void SetTile(int x, int y, char chr) => base.SetTile(x,y,(uint)chr);
    public void SetTileRect(int x0, int y0, int x1, int y1, char chr) {
      base.SetTileRect(x0, y0, x1, y1, (uint)chr);
    }

  }

  //A tile map. One texture (spritesheet) can go on this texture.
  public class TextureGrid {
    public int W {get; private set;}
    public int H {get; private set;}

    //Orientation of cells is:
    //0 1 2 3 4 
    //5 6 7 8 9 

    //Orientation of corners is:
    // 0 1
    // 3 2
    private VertexArray varray;
    private VertexArray bgvarray;     //For the background.
    private RenderStates renderStates;

    private bool dirty = true;

    //private uint[,] textureIDs;
    private TileData[,] tileData;

    public SpriteSheet SpriteSheet {get; private set;}

    //public int TileWidth() => SpriteSheet.tileW;
    //public int TileHeight() => SpriteSheet.tileH;

    public XY TileCoordsFromPixels(double pX, double pY) {
      int iX = SpriteSheet.tileW > 0 ? (int)(pX / SpriteSheet.tileW) : -1;
      int iY = SpriteSheet.tileH > 0 ? (int)(pY / SpriteSheet.tileH) : -1;
      return new XY(iX, iY);
    }

    public void Resize(int newWidth, int newHeight, uint defaultTile = 0) {
      if (newWidth == W && newHeight == H) { return; }
      if (newWidth < 0 || newHeight < 0) { throw new StarExcept("Error: Tried to give a Texturegrid a negative size!"); }

      varray = new VertexArray(PrimitiveType.Quads, (uint)(newWidth*newHeight*4));
      bgvarray = new VertexArray(PrimitiveType.Quads, (uint)(newWidth*newHeight*4));

      var newTileData = new TileData[newHeight, newWidth];

      //Copy over the old array; fill in new areas with the default value.
      for (int y = 0; y < newHeight; ++y) {
        for (int x = 0; x < newWidth; ++x) {
          //newTextureIDs[y,x] = (x < W && y < H) ? textureIDs[y,x] : defaultTile;
          if (x < W && y < H) {
            newTileData[y,x] = tileData[y,x];
          } else {
            var newTile = new TileData();
            //newTile.tileID = defaultTile;
            newTile.ChangeTile(defaultTile);
            newTileData[y,x] = newTile;
          }
        }
      }

      //textureIDs = newTextureIDs;
      tileData = newTileData;
      W = newWidth;
      H = newHeight;
      dirty = true;
    }



    public void UpdateVertexArray() {
      int tpw = SpriteSheet.tileW;
      int tph = SpriteSheet.tileH;
      for (uint y = 0; y < H; ++y) {
        for (uint x = 0; x < W; ++x) {
          //uint tile = textureIDs[y,x];
          TileData td = tileData[y,x];
          uint tile = td.tileID;
          IntRect rect = SpriteSheet.GetSpriteBounds((int)tile);

          float x0 = tpw * x;
          float x1 = x0 + tpw;
          float y0 = tph * y;
          float y1 = y0 + tph;

          float tx0 = rect.Left;
          float tx1 = rect.Left + rect.Width;
          float ty0 = rect.Top;
          float ty1 = rect.Top + rect.Height;

          Color color = td.fgColor;

          uint start = (uint)((y*W + x)*4);

          varray[start  ] = new Vertex(new Vector2f(x0,y0), color, new Vector2f(tx0, ty0));
          varray[start+1] = new Vertex(new Vector2f(x1,y0), color, new Vector2f(tx1, ty0));
          varray[start+2] = new Vertex(new Vector2f(x1,y1), color, new Vector2f(tx1, ty1));
          varray[start+3] = new Vertex(new Vector2f(x0,y1), color, new Vector2f(tx0, ty1));

          //Now do the background.
          Color bgColor = td.bgColor;
          bgvarray[start  ] = new Vertex(new Vector2f(x0,y0), bgColor, new Vector2f(tx0, ty0));
          bgvarray[start+1] = new Vertex(new Vector2f(x1,y0), bgColor, new Vector2f(tx1, ty0));
          bgvarray[start+2] = new Vertex(new Vector2f(x1,y1), bgColor, new Vector2f(tx1, ty1));
          bgvarray[start+3] = new Vertex(new Vector2f(x0,y1), bgColor, new Vector2f(tx0, ty1));

        }
      }

      dirty = false;
    }

    public void SetFGColor(int x, int y, Color color) {
      if (x < 0 || y < 0 || x >= W || y >= H) { throw new StarExcept($"Error: cannot set FG color at coordinates {x},{y} - out of bounds!"); }

      dirty = tileData[y,x].ChangeFGColor(color) || dirty;
    }

    public void SetBGColor(int x, int y, Color? color) {
      if (x < 0 || y < 0 || x >= W || y >= H) { throw new StarExcept($"Error: cannot set BG color at coordinates {x},{y} - out of bounds!"); }

      if (color == null) return;

      dirty = tileData[y,x].ChangeBGColor((Color)color) || dirty;
    }

    private void SetColorRect(int x0, int y0, int x1, int y1, Color color, bool fg) {
      x0 = Math.Max(x0,0);
      y0 = Math.Max(y0,0);
      x1 = Math.Max(x1,0);
      y1 = Math.Max(y1,0);

      //Swap the values if they're out of order. So that x0,y0 is top left and x1,y1 is bot right
      if (y1 < y0) { Util.Swap(ref y0, ref y1); }
      if (x1 < x0) { Util.Swap(ref x0, ref x1); }

      //Return (coloring nothing) unless at least one value is in range.
      if (x0 >= W || y0 >= H) { return; }

      //Cut the parameters down to where we can use them.
      x1 = Math.Min(x1, W-1);
      y1 = Math.Min(y1, H-1);

      //Update the FG or BG.
      for (int y = y0; y <= y1; ++y) {
        for (int x = x0; x <= x1; ++x) {
          bool changedColor = fg ? tileData[y,x].ChangeFGColor(color) : tileData[y,x].ChangeBGColor(color);
          dirty = changedColor || dirty;
        }
      }
    }

    public void SetFGColorRect(int x0, int y0, int x1, int y1, Color fg) {
      SetColorRect(x0, y0, x1, y1, fg, true);
    }

    public void SetBGColorRect(int x0, int y0, int x1, int y1, Color bg) {
      SetColorRect(x0, y0, x1, y1, bg, false);
    }



    public void SetTile(int x, int y, uint tile) {
      if (x < 0 || y < 0 ||x >= W || y >= H) { throw new StarExcept($"Error: cannot set tile at coordinates {x},{y} - out of bounds!"); }

      bool changed = tileData[y,x].ChangeTile(tile);
      dirty = dirty || changed;
    }

    public void SetTileRect(int x0, int y0, int x1, int y1, uint tile) {
      x0 = Math.Max(x0,0);
      y0 = Math.Max(y0,0);
      x1 = Math.Max(x1,0);
      y1 = Math.Max(y1,0);

      //Swap the values if they're out of order. So that x0,y0 is top left and x1,y1 is bot right
      if (y1 < y0) { Util.Swap(ref y0, ref y1);}
      if (x1 < x0) { Util.Swap(ref x0, ref x1);}

      //Return (painting nothing) unless at least one value is in range.
      if (x0 >= W || y0 >= H) { return; }

      //Cut the parameters down to where we can use them.
      x1 = Math.Min(x1, W-1);
      y1 = Math.Min(y1, H-1);

      //Paint
      for (int y = y0; y <= y1; ++y) {
        for (int x = x0; x <= x1; ++x) {
          dirty = tileData[y,x].ChangeTile(tile) || dirty;
        }
      }
    }

    public TextureGrid(SpriteSheet spritesheet, int w, int h) {
      if (spritesheet == null) { throw new StarExcept("Error: spritesheet is null."); }
      this.SpriteSheet = spritesheet;

      this.W = w;
      this.H = h;

      varray = new VertexArray(PrimitiveType.Quads, (uint)(W*H*4));
      bgvarray = new VertexArray(PrimitiveType.Quads, (uint)(W*H*4));
      renderStates = new RenderStates(spritesheet.tex);

      tileData = new TileData[H,W];

      //textureIDs = new uint[H,W];
      for (int y = 0; y < H; ++y) {
        for (int x = 0; x < W; ++x) {
          //textureIDs[y,x] = (uint)(x+ 16*y);
          var td = new TileData();
          //td.tileID = (uint)(x + 16*y);
          //td.ChangeTile((uint)(x + 16*y));
          td.ChangeTile(0);
          tileData[y,x] = td;
        }
      }

      dirty = true;
      UpdateVertexArray();
    }


    public void DrawToRenderTexture(RenderTexture target) {
      if (dirty) { UpdateVertexArray(); }
      bgvarray.Draw(target, RenderStates.Default);
      varray.Draw(target, renderStates);
    }

  }
}