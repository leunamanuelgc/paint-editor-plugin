using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class MoveCommand : ACommand
    {
        private Layer layer;
        private Vector2 delta;
        public MoveCommand(Layer layer, Vector2 delta)
        {
            this.layer = layer;
            this.delta = delta;
        }

        public override bool Execute()
        {            
            layer.Move(delta);
            layer.AddOffset(delta);
            return true;
        }
    }
}
