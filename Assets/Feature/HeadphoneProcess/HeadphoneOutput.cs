using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hsinpa.Headphone { 
    public class HeadphoneOutput
    {
        private Camera m_camera;

        public HeadphoneOutput(Camera p_camera) {
            this.m_camera = p_camera;
        }

        public Texture2D TakeCameraAsTex2D(int width, int height) {
            var tempTex = RenderTexture.GetTemporary(width, height,0, RenderTextureFormat.ARGB32);

            this.m_camera.targetTexture = tempTex;
            this.m_camera.Render();
            this.m_camera.targetTexture = null;

            Texture2D outputTexture = TextureUtility.TextureToTexture2D(tempTex);

            RenderTexture.ReleaseTemporary(tempTex);

            return outputTexture;
        }

        public void SaveTex2DToPath(Texture2D outputTex, string path) {
            byte[] bytes = outputTex.EncodeToJPG();
            File.WriteAllBytes(path, bytes);

            UnityEngine.Object.Destroy(outputTex); //Release memory
        }
    }
}
