
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Utility.Algorithm {
    public class GeneralDataStructure {

        [System.Serializable]
        public struct AreaStruct {
            public int x;
            public int y;
            public int width;
            public int height;

            public float x_ratio;
            public float y_ratio;
            public float width_ratio;
            public float height_ratio;

            public int id;
            public int area => width * height;
        }
    }
}