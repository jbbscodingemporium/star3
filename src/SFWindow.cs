using SFML.System;
using SFML.Window;
using SFML.Graphics;

namespace Star {

  class SFWindow {
    private VideoMode vm;     //Struct for video modes

    private RenderWindow window;    //The handler for the window itself

    private Sprite bufferSprite;
    public RenderTexture buffer;    //The buffer that gets drawn to before the window is updated.

    private uint W => vm.Width;
    private uint H => vm.Height;

    public bool IsOpen => window != null && window.IsOpen;

    private int scaleX = 2;
    private int scaleY = 2;

    public SFWindow(VideoMode vm, string title = "Window") { 
      this.vm = vm;

      window = new RenderWindow(this.vm, title);
      window.Closed += (obj, e) => { window.Close(); };

      bufferSprite = new Sprite();
      bufferSprite.Scale = new Vector2f(2,2);

      buffer = new RenderTexture(W,H);
    }

    public Window GetSFMLWindow() => window;

    public void DispatchEvents() => window.DispatchEvents();

    public void DrawToBuffer(Drawable drawable) => drawable.Draw(buffer, RenderStates.Default);

    public bool HasFocus() => window.HasFocus();

    public XY ScreenPointToUnscaledPoint(XY screen) {
      return new XY(screen.x / scaleX, screen.y / scaleY);
    }

    public void DrawToWindow() {
      window.Clear();

      buffer.Display();                         //Update the texture.
      bufferSprite.Texture = buffer.Texture;

      window.Draw(bufferSprite);                //Copy the render texture to the window.

      window.Display();                         //Actually display the window.
    }

  }

}