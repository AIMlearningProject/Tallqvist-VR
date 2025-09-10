// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using System.Collections.Generic;

namespace Pcx
{
    /// A container class optimized for compute buffer.
    public sealed class PointCloudData : ScriptableObject
    {
        #region Public properties

        /// Byte size of the point element.
        public const int elementSize = sizeof(float) * 4;

        /// Number of points.
        public int pointCount
        {
            get { return _pointData.Length; }
        }

        /// Get access to the compute buffer that contains the point cloud.
        public ComputeBuffer computeBuffer
        {
            get
            {
                if (_pointBuffer == null)
                {
                    _pointBuffer = new ComputeBuffer(pointCount, elementSize);
                    _pointBuffer.SetData(_pointData);
                }
                return _pointBuffer;
            }
        }

        #endregion

        #region ScriptableObject implementation

        ComputeBuffer _pointBuffer;

        void OnDisable()
        {
            if (_pointBuffer != null)
            {
                _pointBuffer.Release();
                _pointBuffer = null;
            }
        }

        #endregion

        #region Serialized data members

        [System.Serializable]
        struct Point
        {
            public Vector3 position;
            public uint color;
        }

        [SerializeField] Point[] _pointData;

        #endregion

        #region Editor functions

#if UNITY_EDITOR

        static uint EncodeColor(Color c)
        {
            const float kMaxBrightness = 16;

            var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
            y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

            var rgb = new Vector3(c.r, c.g, c.b);
            rgb *= 255 * 255 / (y * kMaxBrightness);

            return ((uint)rgb.x) |
                   ((uint)rgb.y << 8) |
                   ((uint)rgb.z << 16) |
                   ((uint)y << 24);
        }

        public void Initialize(List<Vector3> positions, List<Color32> colors)
        {
            _pointData = new Point[positions.Count];
            for (var i = 0; i < _pointData.Length; i++)
            {
                _pointData[i] = new Point
                {
                    position = positions[i],
                    color = EncodeColor(colors[i])
                };
            }
        }

#endif
        //This can be used when using "runtime" options when running from edotor.

        /*public void InitializeRuntime(List<Vector3> positions, List<Color32> colors)
        {
            _pointData = new Point[positions.Count];
            for (int i = 0; i < _pointData.Length; i++)
            {
                _pointData[i] = new Point
                {
                    position = positions[i],
                    color = EncodeColor(colors[i])
                };
            }
        }*/

        #endregion
    }
}

//Old version for runtime use of the ply import tool. (from line 66 onwards)

/*#if UNITY_EDITOR
        public void Initialize(List<Vector3> positions, List<Color32> colors)
        {
            _pointData = new Point[positions.Count];
            for (var i = 0; i < _pointData.Length; i++)
            {
                _pointData[i] = new Point {
                    position = positions[i],
                    color = EncodeColor(colors[i])
                };
            }
        }
#endif

        #endregion

        public void SetPointData(Vector3[] positions, Color32[] colors)
        {
            if (positions.Length != colors.Length)
                throw new ArgumentException("Positions and colors must match in length.");

            _pointData = new Point[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                _pointData[i] = new Point
                {
                    position = positions[i],
                    color = EncodeColor(colors[i])
                };
            }
        }

        private static uint EncodeColor(Color32 c)
        {
            const float kMaxBrightness = 16f;
            float y = Mathf.Max(c.r, Mathf.Max(c.g, c.b)) / 255f;
            y = Mathf.Clamp(Mathf.Ceil(y * 255f / kMaxBrightness), 1, 255);
            Vector3 rgb = new Vector3(c.r, c.g, c.b) * (255f * 255f / (y * kMaxBrightness));
            return ((uint)rgb.x) | ((uint)rgb.y << 8) | ((uint)rgb.z << 16) | ((uint)y << 24);
        }
    }
}*/
