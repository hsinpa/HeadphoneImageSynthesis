using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Hsinpa.Utility.Algorithm {
    public class SegmentationAlgorithm
    {
        private float _threshold_area;
        private int _width, _height;
        private PixelStruct[] _pixelStructArray;

        private int incrementalNewIndex = 1;
        private int _offsetX, _offsetY;

        private readonly Vector2Int TopRightOffset = new Vector2Int(1, -1);
        private readonly Vector2Int TopLeftOffset = new Vector2Int(-1, -1);
        private readonly Vector2Int TopOffset = new Vector2Int(0, -1);
        private readonly Vector2Int LeftOffset = new Vector2Int(-1, 0);


        private Dictionary<int, List<PixelStruct>> pixelLookupTable = new Dictionary<int, List<PixelStruct>>();

        public SegmentationAlgorithm(float threshold_area, int width, int height, int offsetX, int offsetY)
        {
            this._threshold_area = threshold_area;
            this.SetSize(width, height);
            this._offsetX = offsetX;
            this._offsetY = offsetY;
        }

        public void SetSize(int width, int height) {
            this._width = width;
            this._height = height;

            this._pixelStructArray = new PixelStruct[width * height];
        }

        public List<GeneralDataStructure.AreaStruct>  FindAreaStruct(Color[] colors) {
            Dispose();

            for (int x = 0; x < this._width; x++) {
                for (int y = 0; y < this._height; y++) {
                    ProcessPixel(x, y, colors);
                }
            }

            return FilterAreaStruct(pixelLookupTable, _threshold_area);
        }

        public void Dispose()
        {
            int totalLength = this._width * this._height;
            var parallelResult = Parallel.For(0, totalLength, (i) => {
                _pixelStructArray[i].unique_id = 0;
            });

            pixelLookupTable.Clear();
            incrementalNewIndex = 1;
        }

        private void ProcessPixel(int pos_x, int pos_y, Color[] colors) {
            int index = GetPixelIndex(pos_x, pos_y);

            Color selfColor = colors[index];
            int selfBlock = (selfColor.r > 0.5f) ? 1 : 0; // Binary

            if (selfBlock == 0) return;

            PixelStruct top_pixel = FindPixelByPos(pos_x, pos_y, TopOffset),
                        left_pixel = FindPixelByPos(pos_x, pos_y, LeftOffset),
                        topleft_pixel = FindPixelByPos(pos_x, pos_y, TopLeftOffset),
                        topright_pixel = FindPixelByPos(pos_x, pos_y, TopRightOffset);

            _pixelStructArray[index].index = index;
            _pixelStructArray[index].x = pos_x;
            _pixelStructArray[index].y = pos_y;
            _pixelStructArray[index].unique_id = incrementalNewIndex;

            MoveToSegment(_pixelStructArray[index], incrementalNewIndex);

            if ((top_pixel.unique_id > 0 || left_pixel.unique_id > 0 || topleft_pixel.unique_id > 0 || topright_pixel.unique_id > 0))
            {
                ProcessCrossRelatePixel(top_pixel, left_pixel, topleft_pixel, topright_pixel, _pixelStructArray[index]);
                return;
            }
            incrementalNewIndex++;
        }

        private void ProcessCrossRelatePixel(params PixelStruct[] pixels) {
            int target_segment_id = int.MaxValue;

            foreach (PixelStruct pixel in pixels)
            {
                if (pixel.unique_id > 0 && pixel.unique_id < target_segment_id)
                    target_segment_id = pixel.unique_id;
            }

            if (target_segment_id <= 0) return;

            foreach (PixelStruct pixel in pixels)
            {
                if (pixel.unique_id > 0 && pixel.unique_id != target_segment_id)
                    MoveToSegment(pixel.unique_id, target_segment_id);
            }
        }

        private PixelStruct FindPixelByPos(int pos_x, int pos_y, Vector2Int offset) {
            int newPosX = pos_x + offset.x, 
                newPosY = pos_y + offset.y,
                index = GetPixelIndex(newPosX, newPosY);

            bool insideBoundary = InsideBoundary(newPosX, newPosY);

            if (!insideBoundary) return default(PixelStruct);

            return _pixelStructArray[index];
        }

        private void MoveToSegment(PixelStruct pixelStruct, int target_segment_id) {
            pixelLookupTable = Hsinpa.Utility.UtilityFunc.SetListDictionary(pixelLookupTable, target_segment_id, pixelStruct);
        }

        private void MoveToSegment(int original_segment_id, int target_segment_id) {
            //Set all segment id to another (Grid)
            //int totalLength = this._width * this._height;
            //var parallelResult = Parallel.For(0, totalLength, (i) => {
            //    if (pixelStructArray[i].unique_id == original_segment_id)
            //        pixelStructArray[i].unique_id = target_segment_id;
            //});

            //In Dictionary
            if (pixelLookupTable.TryGetValue(original_segment_id, out var o_pixelList)) {
                //Set all segment id to another (Grid)
                foreach (var pixelStruct in o_pixelList)
                    _pixelStructArray[pixelStruct.index].unique_id = target_segment_id;

                if (pixelLookupTable.TryGetValue(target_segment_id, out var t_pixelList))
                {
                    t_pixelList.AddRange(o_pixelList);
                    pixelLookupTable[target_segment_id] = t_pixelList;
                }
            }

            //Remove the original map
            pixelLookupTable.Remove(original_segment_id);
        }

        private List<GeneralDataStructure.AreaStruct> FilterAreaStruct(Dictionary<int, List<PixelStruct>> pixelLookupTable, float area_threshold) {
            List<GeneralDataStructure.AreaStruct> areaStructs = new List<GeneralDataStructure.AreaStruct>();
            //Debug.Log("pixelLookupTable length " + pixelLookupTable.Count);

            foreach (var pixelKeyPair in pixelLookupTable) {
                GeneralDataStructure.AreaStruct areaStruct = new GeneralDataStructure.AreaStruct();
                areaStruct.id = pixelKeyPair.Key;

                if (pixelKeyPair.Value == null || pixelKeyPair.Value.Count <= 0) continue;

                var firstPixel = pixelKeyPair.Value[0];
                int top = firstPixel.y, bottom = firstPixel.y, left = firstPixel.x, right = firstPixel.x;

                foreach (PixelStruct pixelStruct in pixelKeyPair.Value) {

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
                areaStruct.x = Mathf.CeilToInt( Mathf.Lerp(left, right, 0.5f) ) ;
                areaStruct.y = Mathf.CeilToInt(Mathf.Lerp(bottom, top, 0.5f)) ;

                //Debug.Log("Area " + areaStruct.area);

                if (areaStruct.area >= area_threshold) {
                    //Debug.Log(areaStruct.id + ", left " + left + ", right " + right + ", top " + top + ", bottom " + bottom);
                    areaStructs.Add(areaStruct);
                }
            }

            return areaStructs;
        }


        private bool InsideBoundary(int pos_x, int pos_y) {
            return pos_x >= 0 && pos_x < this._width && pos_y >= 0 && pos_y < this._height;
        }

        private int GetPixelIndex(int width, int height) {
            return width + (this._width * height);
        }

        private struct PixelStruct {
            public int index;
            public int unique_id;
            public int x;
            public int y;

            public bool Valid => unique_id > 0;
        }

    }
}
