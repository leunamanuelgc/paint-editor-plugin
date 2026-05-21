using System;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.PaintEditor
{
    public class Layer
    {

        public static string iconTextureOn = "d_VisibilityOn";
        public static string iconTextureOff = "d_VisibilityOff";

        public Rect rect {  get; set; }

        public Vector2 realSize { get; set; }

        public bool isEnabled { get; set; }

        public bool isSelected { get; set; }

        public string name { get; set; }

        public CustomRenderTexture rTexture;

        #region BlendModes

        public enum LayerBlendMode
        {
            normal,
            additive,
            soft_additive,
            multiplicative,
            multiplicative_2x,
            subtract,
            reverse_subtract,
            min,
            max,
        }

        public Material blendMaterial;
        public CustomRenderTexture blendedTexture;
        private Shader blendShader;
        public LayerBlendMode blendMode;
        private string shaderName = "PaintEditorPlugin/BlendTexture";
        public int blendModeIdx;

        #endregion

        public Layer(int num, Rect rect)
        {
            this.rect = new Rect(rect.position, rect.size * PaintEditorPlugin.Instance.GetZoomLevel());
            this.realSize = rect.size;
            this.isEnabled = true;
            this.name = "Layer " + num;

            InitializeTexture(out rTexture, (int)rect.width, (int)rect.height);
            InitializeTexture(out blendedTexture, (int)rect.width, (int)rect.height);

            PanCommand.OnPanMove += Move;
            Zoom.OnZoomLevelChange += Resize;
            blendShader = Shader.Find(shaderName);

            blendMaterial = new Material(blendShader);
            blendMaterial.SetTexture("_MainTexture", rTexture);
            blendMaterial.SetFloat("_SrcFactorA", 5);
            blendMaterial.SetFloat("_DstFactorA", 10);
            blendMaterial.SetFloat("_OpAlpha", 0);
            blendModeIdx = 0;
            Utils.onBlendModeChange += ChangeBlendMode;
            ChangeBlendMode();
        }

        ~Layer()
        {
            Release();
        }

        public void InitializeTexture(out CustomRenderTexture texture, int width, int height)
        {
            texture = new CustomRenderTexture(width, height, RenderTextureFormat.ARGB32);
            texture.filterMode = FilterMode.Point;
            texture.enableRandomWrite = true;
            texture.Create();
        }

        public void Move(Vector2 delta)
        {
            rect = new Rect(rect.x + delta.x, rect.y + delta.y, rect.width, rect.height);
        }

        public void Resize(float zoomLevel)
        {
            rect = new Rect(rect.position, realSize * zoomLevel);
        }

        public void Release()
        {
            rTexture.Release();
            rTexture = null;

            Zoom.OnZoomLevelChange -= Resize;
            PanCommand.OnPanMove -= Move;
            Utils.onBlendModeChange -= ChangeBlendMode;
        }

        public void CopyToLayer(Layer layer)
        {
            layer.name = this.name;
            layer.isSelected = this.isSelected;
            layer.isEnabled = this.isEnabled;
            layer.realSize = this.realSize;
            layer.rect = this.rect;
            Graphics.CopyTexture(this.rTexture, layer.rTexture);
        }

        public void ChangeBlendMode()
        {
            switch (blendModeIdx)
            {   
                case 0:
                    //Normal
                    blendMode = LayerBlendMode.normal;
                    blendMaterial.SetFloat("_SrcFactor", 5);
                    blendMaterial.SetFloat("_DstFactor", 10);
                    blendMaterial.SetFloat("_OpColor", 0);
                    
                    break;
                case 1:
                    //Additive
                    blendMode = LayerBlendMode.additive;
                    blendMaterial.SetFloat("_SrcFactor", 1);
                    blendMaterial.SetFloat("_DstFactor", 1);
                    blendMaterial.SetFloat("_OpColor", 0);
                    
                    break;
                case 2:
                    //Soft Additive
                    blendMode = LayerBlendMode.soft_additive;
                    blendMaterial.SetFloat("_SrcFactor", 4);
                    blendMaterial.SetFloat("_DstFactor", 1);
                    blendMaterial.SetFloat("_OpColor", 0) ;
                    
                    break;
                case 3:
                    //Multiplicative
                    blendMode = LayerBlendMode.multiplicative;
                    blendMaterial.SetFloat("_SrcFactor", 2);
                    blendMaterial.SetFloat("_DstFactor", 0);
                    blendMaterial.SetFloat("_OpColor", 0);

                    break;
                case 4:
                    //Multiplicative x2
                    blendMode = LayerBlendMode.multiplicative_2x;
                    blendMaterial.SetFloat("_SrcFactor", 2);
                    blendMaterial.SetFloat("_DstFactor", 3);
                    blendMaterial.SetFloat("_OpColor", 0);
                    
                    break;
                case 5:
                    //Subtract
                    blendMode = LayerBlendMode.subtract;
                    blendMaterial.SetFloat("_SrcFactor", 3);
                    blendMaterial.SetFloat("_DstFactor", 2);
                    blendMaterial.SetFloat("_OpColor", 1);

                    break;
                case 6:
                    //Reverse Subtract
                    blendMode = LayerBlendMode.reverse_subtract;
                    blendMaterial.SetFloat("_SrcFactor", 3);
                    blendMaterial.SetFloat("_DstFactor", 2);
                    blendMaterial.SetFloat("_OpColor", 2);

                    break;
                case 7:
                    //Min
                    blendMode = LayerBlendMode.min;
                    blendMaterial.SetFloat("_SrcFactor", 3);
                    blendMaterial.SetFloat("_DstFactor", 2);
                    blendMaterial.SetFloat("_OpColor", 3);

                    break;
                case 8:
                    //Max
                    blendMode = LayerBlendMode.max;
                    blendMaterial.SetFloat("_SrcFactor", 3);
                    blendMaterial.SetFloat("_DstFactor", 2);
                    blendMaterial.SetFloat("_OpColor", 4);

                    break;
            }

            Graphics.Blit(rTexture, blendedTexture, blendMaterial);
        }
    }
}