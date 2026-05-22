using UnityEngine;

namespace PaintEditorExtension
{
    public class ZoomCommand : ACommand
    {
        Zoom zoom;
        float zoomToAdd;
        EventType eType;
        public ZoomCommand(Zoom zoom, float zoomToAdd, EventType eType)
        {
            this.zoom = zoom;
            this.zoomToAdd = zoomToAdd;
            this.eType = eType;
        }

        public override bool Execute()
        {
            if (eType == EventType.MouseDown)
            {
                zoom.SetInitZoom();
            }
            else
            {
                if(eType == EventType.ScrollWheel)
                {
                    zoom.AddZoom(zoomToAdd * 0.5f);
                }
                else
                {
                    zoom.AddZoom(zoomToAdd * 0.05f);
                }
                
            }
            return false;
        }
    }
}
