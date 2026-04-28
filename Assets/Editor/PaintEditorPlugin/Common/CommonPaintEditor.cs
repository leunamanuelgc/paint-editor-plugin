using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class CommonPaintEditor
    {
        public static Vector2Int PosInRectInt(Vector2 pos, Rect rect)
        {
            float new_x = pos.x - rect.x;
            float new_y = rect.height - (pos.y - rect.y);

            return new Vector2Int((int)new_x, (int)new_y);
        }

        public static Vector2Int DeltaInt()
        {
            Vector2Int delta = new Vector2Int((int)Event.current.delta.x, (int)Event.current.delta.y);
            return delta;
        }

        public static Vector2 ConvertPos(Vector2 pos, Rect r, Vector2 size)
        {
            Vector2 convertion = new Vector2(size.x / r.width, size.y / r.height);

            return new Vector2(pos.x * convertion.x, pos.y * convertion.y);
        }
    }
}