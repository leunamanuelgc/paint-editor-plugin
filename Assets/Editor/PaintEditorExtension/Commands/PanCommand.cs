using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class PanCommand : ACommand
    {
        Vector2 delta;
        Rect canvasRect;
        Rect[] limits;

        public static event Action<Vector2> OnPanMove;

        public PanCommand(Vector2 delta, Rect canvasRect, params Rect[] limits)
        {
            this.delta = delta;
            this.canvasRect = canvasRect;
            this.limits = limits;
        }

        public override bool Execute()
        {
            Rect newRect = new Rect(canvasRect.position + delta, canvasRect.size);

            if (limits.Length > 0)
            {
                bool statement = newRect.xMin < limits[0].width - 50 && newRect.yMin < limits[0].height - 50 && newRect.xMax > 50 && newRect.yMax > 50;

                if (!statement) return false;
            }

            OnPanMove?.Invoke(delta);
            return false;
        }
    }
}
