using Godot;

public static class CanvasItemExtensions
{
    public static void DrawCenteredString(this CanvasItem item, Font font, Vector2 pos, string text)
    {
        var stringSize = font.GetStringSize(text);
        var centeredPos = new Vector2(pos.X - stringSize.X / 2, pos.Y + stringSize.Y / 2);
        item.DrawString(font, centeredPos, text);
    }
}
