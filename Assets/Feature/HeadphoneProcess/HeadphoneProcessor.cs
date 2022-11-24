using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hsinpa.Utility;
using System.IO;
using System.Threading.Tasks;
using Hsinpa.Utility.Algorithm;

namespace Hsinpa.Headphone
{
    public class HeadphoneProcessor : MonoBehaviour
    {
        [SerializeField]
        private RectTransform CanvasTransform;

        [SerializeField]
        private RawImage RawColorImage;

        [SerializeField]
        private Image Headphone_Core_A;

        [SerializeField]
        private Image Headphone_Core_B;

        [SerializeField]
        private Image Headphone_Overband;

        [Header("Sprite")]
        [SerializeField]
        private Sprite Headphone_Core_L_Sprite;

        [SerializeField]
        private Sprite Headphone_Core_R_Sprite;

        [SerializeField]
        private Sprite Headphone_Band_L_Sprite;

        [SerializeField]
        private Sprite Headphone_Band_R_Sprite;

        [Header("Debug")]
        [SerializeField]
        private RectTransform DebugCanvas;

        [SerializeField]
        private RawImage TestElementA;

        private string[] folder_array = new string[] {HeadphoneStatic.Segment.LSegment30, HeadphoneStatic.Segment.RSegment30 };

        private HeadphoneSegment m_headphoneSegment;

        int m_folder_index;

        int canvas_center_x;
        int canvas_cneter_y;

        private Camera m_camera;
        private HeadphoneOutput m_headphoneOutput;

        private const float StepTime = 1; //sec
        private float _recordStepTime; 

        private bool _internalProcessFlag = false;
        private string _flagFileFullPath;
        private void Start()
        {
            m_camera = Camera.main;
            m_headphoneSegment = new HeadphoneSegment();
            m_headphoneOutput = new HeadphoneOutput(m_camera);

            canvas_center_x = (int) (CanvasTransform.sizeDelta.x * 0.5f);
            canvas_cneter_y = (int)(CanvasTransform.sizeDelta.y * 0.5f);

            _flagFileFullPath = Path.Combine(Application.streamingAssetsPath, HeadphoneStatic.Files.FlagFile);

        }

        private void Update()
        {
            if (_internalProcessFlag) return;

            if (Time.time > _recordStepTime) {

                _recordStepTime = Time.time + StepTime;

                string flag = IOUtility.GetFileText(_flagFileFullPath);

                if (flag == "0")
                    _ = ProcessFolder(0);

            }
        }

        private async Task ProcessFolder(int index) {

            _internalProcessFlag = true;

            //Hide core b until c_0;
            Headphone_Core_B.enabled = false;

            if (index >= folder_array.Length) {

                IOUtility.SaveFileText(_flagFileFullPath, "1");

                _internalProcessFlag = false;

                Debug.Log("Process done");
                return;
            }

            try
            {
                SetSpirteByDirection(folder_array[index]);
                string fullDirPath = Path.Combine(Application.streamingAssetsPath, HeadphoneStatic.Files.Input, folder_array[index]);
                string rawColorImagePath = Path.Combine(fullDirPath, HeadphoneStatic.Files.Raw);
                string segVisionImagePath = Path.Combine(fullDirPath, HeadphoneStatic.Files.SegVision);

                var colorTex = TextureUtility.GetTexture2DFromPath(rawColorImagePath);
                float aspectRatio = colorTex.width / (float)colorTex.height;
                
                RawColorImage.texture = colorTex;
                RawColorImage.rectTransform.sizeDelta = new Vector2(CanvasTransform.sizeDelta.y * aspectRatio, RawColorImage.rectTransform.sizeDelta.y);

                Debug.Log($"ColorTex width {colorTex.width}, height {colorTex.height}");
                Debug.Log($"RawColorImage size {RawColorImage.rectTransform.sizeDelta}");

            int visionWidth = (int)(aspectRatio * HeadphoneStatic.Segment.Height);

            if (HeadphoneStatic.TargetColorDict.TryGetValue(folder_array[index], out Color targetColor)) { 
            
                HeadphoneSegment.SegmentStruct segmentStruct =  await m_headphoneSegment.ProcessVisionSegment(segVisionImagePath, visionWidth, HeadphoneStatic.Segment.Height, targetColor);
                m_headphoneSegment.SetDebugTexture(TestElementA);

                ProcessEarphonePosition(segmentStruct, colorTex.width, (int)CanvasTransform.sizeDelta.y);
                ProcessSideEarBand(segmentStruct.topHeadPositionRatio, Headphone_Core_A, colorTex.width, (int)CanvasTransform.sizeDelta.y);
                
                //Output
                DebugCanvas.gameObject.SetActive(false);
                IOUtility.CreateDirectoryIfNotExist(Application.streamingAssetsPath, HeadphoneStatic.Files.Output, folder_array[index]);
                string outputPath = Path.Combine(Application.streamingAssetsPath, HeadphoneStatic.Files.Output, folder_array[index], HeadphoneStatic.Files.OutputFileName);
                this.m_headphoneOutput.SaveTex2DToPath(this.m_headphoneOutput.TakeCameraAsTex2D(colorTex.width, colorTex.height), outputPath);

                await Task.Delay(100);

                UnityEngine.Object.Destroy(colorTex); //Release memory
                DebugCanvas.gameObject.SetActive(true);
                _ = ProcessFolder(index + 1);
            }
            }
            catch {

                _ = ProcessFolder(index + 1);

            }
        }

        private void ProcessEarphonePosition(HeadphoneSegment.SegmentStruct segmentStruct, int colorTexWidth, int colorTexHeight) {
            ProcessSingleEarBody(segmentStruct.ear_a, colorTexWidth, colorTexHeight);
        }

        private void ProcessSingleEarBody(GeneralDataStructure.AreaStruct areaStruct, int colorTexWidth, int colorTexHeight) {
            RectTransform coreTransform = Headphone_Core_A.rectTransform;

            float newPositionX = (colorTexWidth * areaStruct.x_ratio) ;
            float newPositionY = colorTexHeight * (areaStruct.y_ratio);

            Debug.Log($"ProcessSingleEarBody newPositionX {newPositionX}, newPositionY {newPositionY}");

            coreTransform.anchoredPosition = new Vector2(newPositionX, newPositionY);
        }

        private void ProcessSideEarBand(Vector2 headBandPosRatio, Image headphoneCoreImg, int colorTexWidth, int colorTexHeight)
        {
            //Place to locate band's lower part
            RectTransform coreTransform = Headphone_Overband.GetComponent<RectTransform>();
            float newPositionX = Mathf.Lerp(headphoneCoreImg.rectTransform.offsetMin.x, headphoneCoreImg.rectTransform.offsetMax.x, 0.5f);
            float newPositionY = Mathf.Lerp(headphoneCoreImg.rectTransform.offsetMin.y, headphoneCoreImg.rectTransform.offsetMax.y, 0.85f);

            float aspectRatio = Headphone_Overband.sprite.rect.width / (float)Headphone_Overband.sprite.rect.height;
            float enlargeRatio = 1.2f;
            int heightOffset = -1; //Band won't usually locate on top of hair
            float expectHeight = (headBandPosRatio.y * colorTexHeight) - newPositionY + heightOffset;
                  expectHeight = Mathf.Clamp(expectHeight, 50, expectHeight);

            float expectWidth = (expectHeight * aspectRatio);

            Debug.Log($"ProcessEarBand aspectRatio {aspectRatio}, width {expectWidth}, height {expectHeight}");

            coreTransform.sizeDelta = new Vector2(expectWidth, expectHeight) * enlargeRatio;
            coreTransform.anchoredPosition = new Vector2(newPositionX, newPositionY);
        }

        private void SetSpirteByDirection(string direction) { 
            switch(direction) {

                case HeadphoneStatic.Segment.LSegment30:
                    SetSpirteToImages(Headphone_Core_L_Sprite, Headphone_Core_A);
                    SetSpirteToImages(Headphone_Band_L_Sprite, Headphone_Overband);
                    break;

                case HeadphoneStatic.Segment.RSegment30:
                    SetSpirteToImages(Headphone_Core_R_Sprite, Headphone_Core_A);
                    SetSpirteToImages(Headphone_Band_R_Sprite, Headphone_Overband);
                    break;

            }
        }

        private void SetSpirteToImages(Sprite sprite, Image destImage)
        {
            destImage.sprite = sprite;
            destImage.rectTransform.pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
        }

    }
}