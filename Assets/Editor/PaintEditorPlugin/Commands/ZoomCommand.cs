namespace UnityEditor.PaintEditor
{
    public class ZoomCommand : ACommand
    {
        Zoom zoom;
        float zoomToAdd;
        public ZoomCommand(Zoom zoom, float zoomToAdd)
        {
            this.zoom = zoom;
            this.zoomToAdd = zoomToAdd;
        }

        public override bool Execute()
        {
            zoom.ChangeZoomLevel(zoomToAdd);
            return false;
        }
    }
}
