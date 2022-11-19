using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.Headphone
{
    public class HeadphoneStatic
    {

        public class Files {
            public const string Input = "Input";
            public const string Output = "Output";

            public const string Raw = "origin.jpg";
            public const string SegVision = "user_seg_vis.png";
            public const string OutputFileName = "try_on_result.jpg";
        }

        public class Segment {
            public const string LSegment30 = "l_30";
            public const string RSegment30 = "r_30";

            public const int Height = 128;

            public static Color LeftEarCol = new Color(0, 1, 1, 1);
            public static Color RightEarCol = new Color(0.2f, 0.6666f, 0.8666f, 1);

            public static Color Hair = new Color(1, 0, 0, 1);
            public static Color Face = new Color(0, 0, 1, 1);
        }

        public static Dictionary<string, Color> TargetColorDict = new Dictionary<string, Color>() {
            { Segment.LSegment30, Segment.LeftEarCol },
            { Segment.RSegment30, Segment.RightEarCol }
        };

    }
}