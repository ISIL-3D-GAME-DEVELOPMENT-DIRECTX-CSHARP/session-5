using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace Sesion2_Lab01 {
    public class RenderCamera {

        public const int PERSPECTIVE = 0;
        public const int ORTHOGRAPHIC = 1;

        private int mCameraType;

        private Matrix mViewNMatrix;
        private Matrix mProjectionNMatrix;
        private Matrix mWorld;
        private Matrix mTransformed;

        private Vector3 mCameraPosition;

        public virtual Vector3 Position {
            get { return mCameraPosition; }
            set { mCameraPosition = value; }
        }

        public Matrix world         { get { return mWorld; } }
        public Matrix projection    { get { return mProjectionNMatrix; } }
        public Matrix view          { get { return mViewNMatrix; } }
        public Matrix transformed   { get { return mTransformed; } }

        public int CameraType       { get { return mCameraType; } }

        public RenderCamera(int width, int height) {
            mCameraType = RenderCamera.ORTHOGRAPHIC;

            mWorld = SimpleMatrix.Identity;
            mProjectionNMatrix = SimpleMatrix.CreateOrthographicOffCenter(0, -width, height, 0, 1.0f, 100.0f);
            mViewNMatrix = SimpleMatrix.CreateLookAt(new Vector3(0, 0, -1.0f), Vector3.Zero, Vector3.UnitY);
        }

        public RenderCamera(NViewport viewport, float z) {
            mCameraType = RenderCamera.PERSPECTIVE;

            mWorld = mViewNMatrix = mProjectionNMatrix = mTransformed = SimpleMatrix.Identity;

            mCameraPosition = Vector3.Zero;
            mCameraPosition.Z = -z;

            float viewAngle = (float)Math.PI / 4.0f;
            float nearPlane = 0.1f;
            float farPlane = 4000000.0f;

            mProjectionNMatrix = SimpleMatrix.CreatePerspectiveFieldOfView(viewAngle,
                viewport.AspectRatio, nearPlane, farPlane);

            UpdateViewMatrix();
        }

        public virtual void UpdateViewMatrix() {
            mViewNMatrix = SimpleMatrix.CreateLookAt(mCameraPosition, Vector3.Zero, Vector3.UnitY);
        }

        public void Update() {
            switch (mCameraType) {
            case RenderCamera.PERSPECTIVE:
                UpdateViewMatrix();
                break;
            }

            Matrix temp = Matrix.Identity;

            Matrix.Multiply(ref mWorld, ref mViewNMatrix, out temp);
            Matrix.Multiply(ref temp, ref mProjectionNMatrix, out mTransformed);
        }
    }
}
