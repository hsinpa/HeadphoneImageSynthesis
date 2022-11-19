using Hsinpa.Utility.Algorithm;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Hsinpa.Headphone {
    public class HeadphoneSegment
    {
        RenderTexture _cacheRenderTexture;
        Texture2D _cacheTexture2D;

        Vector2 _textureSize;

        private readonly Vector2Int TopRightOffset = new Vector2Int(1, 1);
        private readonly Vector2Int TopLeftOffset = new Vector2Int(-1, 1);
        private readonly Vector2Int TopOffset = new Vector2Int(0, 1);
        private readonly Vector2Int LeftOffset = new Vector2Int(-1, 0);

        Dictionary<int, int> _targetColorDict = new Dictionary<int, int>();
        Dictionary<int, List<Vector2Int>> _targetSegmentDict = new Dictionary<int, List<Vector2Int>>();
        int _increment_id = 0;

        public async Task<SegmentStruct> ProcessVisionSegment(string vision_path, int targetWidth, int targetHeight, Color targetEarColor) {
            SegmentStruct segmentStruct = new SegmentStruct();
            _targetColorDict.Clear();
            _targetSegmentDict.Clear();
            _increment_id = 0;
            _textureSize = new Vector2(targetWidth, targetHeight);

            var segVisionRaw = TextureUtility.GetTexture2DFromPath(vision_path);

            if (_cacheRenderTexture == null)
                _cacheRenderTexture = TextureUtility.GetRenderTexture(targetWidth, targetHeight, depth: 0, format: RenderTextureFormat.ARGB32);

            _cacheRenderTexture.Release();

            Graphics.Blit(segVisionRaw, _cacheRenderTexture);

            this._cacheTexture2D = TextureUtility.TextureToTexture2D(_cacheRenderTexture);

            Debug.Log($"scaleDownTex width {this._cacheTexture2D.width}, height { this._cacheTexture2D.height}");

            var rawPixelData = this._cacheTexture2D.GetPixels();

            var areaStructArray = await FindEarAreaStruct(targetWidth, targetHeight, targetEarColor, rawPixelData, errorRate: 0.15f);

            var averageHeadTopPosition = await FindHeadponeBandPosition(targetWidth, targetHeight, step:10, HeadphoneStatic.Segment.Hair, rawPixelData, errorRate: 0.15f);

            if (areaStructArray.Count > 0)
                segmentStruct.ear_a = areaStructArray[0];

            if (areaStructArray.Count > 1)
                segmentStruct.ear_b = areaStructArray[1];

            segmentStruct.topHeadPositionRatio = new Vector2(averageHeadTopPosition.x / targetWidth, averageHeadTopPosition.y / targetHeight);
            Debug.Log($"averageHeadTopPosition {averageHeadTopPosition}, ratio {segmentStruct.topHeadPositionRatio}" );

            return segmentStruct;
        }

        public void SetDebugTexture(UnityEngine.UI.RawImage debugImage) {
            if (_cacheRenderTexture != null)
                debugImage.texture = this._cacheTexture2D;
        }

        private Task<Vector2> FindHeadponeBandPosition(int targetWidth, int targetHeight, int step, Color targetColor, Color[] rawPixels, float errorRate) {
            return Task.Run(() =>
            {
                int stepAmount = Mathf.RoundToInt(targetWidth / (float)step);
                for (int y = targetHeight - 1; y >= 0; y--)
                {
                    for (int x = stepAmount; x < targetWidth; x += stepAmount)
                    {
                        int index = GetPixelIndex(x, y, targetWidth);
                        Color currentColor = rawPixels[index];
                        float diff = ColorDiff(currentColor, targetColor);

                        if (diff > errorRate) continue;

                        return new Vector2(x, y);
                    }
                }

                return new Vector2(targetWidth * 0.5f, targetHeight);
            });
        }

        private Task<List<GeneralDataStructure.AreaStruct>> FindEarAreaStruct(int targetWidth, int targetHeight, Color targetColor, Color[] rawPixels, float errorRate) {
            return Task.Run(() =>
            {
                //Loop from top -> bottom, left -> right

                for (int y = targetHeight - 1; y >= 0; y--)
                {
                    for (int x = 0; x < targetWidth; x++)
                    {
                        int index = GetPixelIndex(x, y, targetWidth);
                        Color currentColor = rawPixels[index];
                        float diff = ColorDiff(currentColor, targetColor);

                        if (diff > errorRate) continue;
                        
                        int id = FindSegmentIDByGroup(x, y, targetWidth, LeftOffset, TopOffset, TopLeftOffset, TopRightOffset);

                        if (id < 0) {
                            _increment_id++;
                            id = _increment_id;
                        }
                        _targetColorDict.Add(index, id);
                        _targetSegmentDict = Hsinpa.Utility.UtilityFunc.SetListDictionary<int, Vector2Int>(_targetSegmentDict, id, new Vector2Int(x, y));
                    }
                }

                return FilterAreaStruct(_targetSegmentDict, 30);
            });
        }


        private int FindSegmentIDByGroup(int x, int y, int width, params Vector2Int[] indexOffsets) {

            foreach (Vector2Int offset in indexOffsets) {
                int find_id = FindSegmentID(x, y, offset, width);

                if (find_id >= 0)
                    return find_id;
            }

            return -1;
        }


        private int FindSegmentID(int x, int y, Vector2Int indexOffset, int width) {
            int index = GetPixelIndex(x + indexOffset.x, y + indexOffset.y, width);
            if (_targetColorDict.TryGetValue(index, out int id))
                return id;

            return -1;
        }

        private float ColorDiff(Color color_a, Color color_b) {
            return Mathf.Sqrt(
                    Mathf.Pow(color_a.r - color_b.r, 2) +
                    Mathf.Pow(color_a.g - color_b.g, 2) +
                    Mathf.Pow(color_a.b - color_b.b, 2)
                   );
        }

        public static int GetPixelIndex(int x, int y, int width)
        {
            return x + (width * y);
        }

        private List<GeneralDataStructure.AreaStruct> FilterAreaStruct(Dictionary<int, List<Vector2Int>> pixelLookupTable, float area_threshold)
        {
            List<GeneralDataStructure.AreaStruct> areaStructs = new List<GeneralDataStructure.AreaStruct>();
            //Debug.Log("pixelLookupTable length " + pixelLookupTable.Count);

            foreach (var pixelKeyPair in pixelLookupTable)
            {
                GeneralDataStructure.AreaStruct areaStruct = new GeneralDataStructure.AreaStruct();
                areaStruct.id = pixelKeyPair.Key;

                if (pixelKeyPair.Value == null || pixelKeyPair.Value.Count <= 0) continue;

                var firstPixel = pixelKeyPair.Value[0];
                int top = firstPixel.y, bottom = firstPixel.y, left = firstPixel.x, right = firstPixel.x;

                foreach (Vector2Int pixelStruct in pixelKeyPair.Value)
                {
                    if (pixelStruct.x <= left)
                        left = pixelStruct.x;

                    if (pixelStruct.x > right)
                        right = pixelStruct.x;

                    if (pixelStruct.y >= top)
                        top = pixelStruct.y;

                    if (pixelStruct.y < bottom)
                        bottom = pixelStruct.y;
                }

                areaStruct.width = right - left;
                areaStruct.height = top - bottom;
                areaStruct.x = Mathf.CeilToInt(Mathf.Lerp(left, right, 0.5f));
                areaStruct.y = Mathf.CeilToInt(Mathf.Lerp(bottom, top, 0.5f));

                areaStruct.x_ratio = areaStruct.x / _textureSize.x;
                areaStruct.y_ratio = areaStruct.y / _textureSize.y;
                areaStruct.width_ratio = areaStruct.width / _textureSize.x;
                areaStruct.height_ratio = areaStruct.height / _textureSize.y;

                //Debug.Log("Area " + areaStruct.area +", Key" + pixelKeyPair.Key);

                if (areaStruct.area >= area_threshold)
                {
                    //Debug.Log(areaStruct.id + ", left " + left + ", right " + right + ", top " + top + ", bottom " + bottom);
                    areaStructs.Add(areaStruct);
                }
            }

            return areaStructs.OrderByDescending( x=>x.area ).ToList();
        }

        public struct SegmentStruct {
            public GeneralDataStructure.AreaStruct ear_a;
            public GeneralDataStructure.AreaStruct ear_b;

            public Vector2 topHeadPositionRatio;
        }
    }
}