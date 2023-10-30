using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;

namespace OPTCAMSim
{
    public class OGLCamera
    {
        float zoom = 1.1f;
        public float cur_zoom = 1f;
        public static Vector3 OZ = new Vector3(0, 0, 1);
        public static Vector3 OX = new Vector3(1, 0, 0);
        public static Vector3 OY = new Vector3(0, 1, 0);
        
        double angleXY, angleZ;
        public int times_changed_sign = 0;
        double sphereR = 0;
        public Vector3 eyePos, targetPos;
        public Vector3 eyePos_def, targetPos_def;

        /// <summary>
        /// eye matrix
        /// </summary>
        public Matrix4 eyeMatrix;
        public OGLCamera()
        {
            eyePos = new Vector3(-10, -10, 30);
            targetPos = new Vector3(50, 50, 0);
            eyeMatrix = Matrix4.LookAt(eyePos, targetPos, OZ);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref eyeMatrix);
        }

        public void Reset(CSGPanel cSGPanel)
        {
            if (cSGPanel != null)
            {
                float rXY = (float)(1.1 * Math.Sqrt(cSGPanel.Width * cSGPanel.Width + cSGPanel.Length * cSGPanel.Length) / 2);
                float y_ = (float)(cSGPanel.Width / 2 - rXY);
                float z_ = (float)(rXY * Math.Tan(45 * Math.PI / 180));
                eyePos = new Vector3(cSGPanel.Length * 0.5f, y_, z_);
                targetPos = new Vector3(cSGPanel.Length / 2, cSGPanel.Width / 2.0f, 0);
            }
            else
            {
                eyePos = eyePos_def;
                targetPos = targetPos_def;
            }
            Vector3 sphereCoord = (eyePos - targetPos);
            sphereR = sphereCoord.Length;
            cur_zoom = 1;
            angleXY = Math.Atan2(sphereCoord.Y, sphereCoord.X);
            angleZ = Math.Atan2(Math.Sqrt(sphereCoord.Y * sphereCoord.Y + sphereCoord.X * sphereCoord.X), sphereCoord.Z);
            eyeMatrix = Matrix4.LookAt(eyePos,
                                                targetPos,
                                               OZ);
            times_changed_sign = 0;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref eyeMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
        }
        public Vector3 GetMoveVector(float step, PreviewKeyDownEventArgs e)
        {
            Vector3 move = new Vector3();
            if (e.KeyCode == Keys.S)
            {
                move.X = (float)Math.Cos(angleXY) * step;
                move.Y = (float)Math.Sin(angleXY) * step;
            }
            if (e.KeyCode == Keys.W)
            {
                move.X = -(float)Math.Cos(angleXY) * step;
                move.Y = -(float)Math.Sin(angleXY) * step;
            }
            if (e.KeyCode == Keys.D)
            {
                move.X = (float)Math.Cos(angleXY + Math.PI / 2) * step;
                move.Y = (float)Math.Sin(angleXY + Math.PI / 2) * step;
            }
            if (e.KeyCode == Keys.A)
            {
                move.X = -(float)Math.Cos(angleXY + Math.PI / 2) * step;
                move.Y = -(float)Math.Sin(angleXY + Math.PI / 2) * step;

            }
            if (e.KeyCode == Keys.Q)
            {
                move.Z = (float)Math.Cos(angleXY + Math.PI / 2) * step;
            }
            if (e.KeyCode == Keys.E)
            {
                move.Z = -(float)Math.Cos(angleXY + Math.PI / 2) * step;
            }
            return move;
        }
        public void RotateCamera(double angleXY_rad, double angleZ_rad)
        {
            angleZ_rad = angleZ_rad * Math.Pow(-1, times_changed_sign);
            //Sphere radius
            if (angleXY_rad == 0 && angleZ_rad == 0)
                return;
            angleXY += angleXY_rad;
            angleZ += angleZ_rad;
            if (angleZ < 0)
            {
                angleZ = Math.Abs(angleZ);
                angleXY += Math.PI;
                times_changed_sign++;
            }
            if (angleZ > Math.PI)
            {
                angleZ = Math.PI - angleZ;
                angleXY += Math.PI;
                times_changed_sign++;
            }

            angleXY = angleXY % (2 * Math.PI);

            RefreshCameraPos();
        }
        public void PanCamera(double angleXY_rad, double angleZ_rad)
        {
            angleZ_rad = angleZ_rad * Math.Pow(-1, times_changed_sign);
            //Sphere radius
            if (angleXY_rad == 0 && angleZ_rad == 0)
                return;
            angleXY += angleXY_rad;
            angleZ += angleZ_rad;
            if (angleZ < 0)
            {
                angleZ = Math.Abs(angleZ);
                angleXY += Math.PI;
                times_changed_sign++;
            }
            if (angleZ > Math.PI)
            {
                angleZ = Math.PI - angleZ;
                angleXY += Math.PI;
                times_changed_sign++;
            }

            angleXY = angleXY % (2 * Math.PI);

            Vector3 sphereCoord = (targetPos - eyePos);
            double R = sphereR / cur_zoom;

            sphereCoord.X = (float)(R * Math.Sin(Math.PI - angleZ) * Math.Cos(Math.PI + angleXY));
            sphereCoord.Y = (float)(R * Math.Sin(Math.PI - angleZ) * Math.Sin(Math.PI + angleXY));
            sphereCoord.Z = (float)(R * Math.Cos(Math.PI - angleZ));

            targetPos = (eyePos + sphereCoord);

            RefreshCameraPos();
        }
        public void MoveCamera(Vector3 move)
        {
            eyePos += move * (float) Math.Pow(-1, times_changed_sign);
            targetPos += move * (float) Math.Pow(-1, times_changed_sign);
            RefreshCameraPos();
        }
        public void Zoom(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                cur_zoom *= zoom;               
            }
            else
            {
                cur_zoom /= zoom;
            }
            RefreshCameraPos();
        }
        public void TopView()
        {
            angleXY = (float)Math.Pow(-1,1+ times_changed_sign) * Math.PI/2;
            angleZ = 1E-5;
            RefreshCameraPos();
        }
        public void RefreshCameraPos()
        {
            Vector3 sphereCoord = (eyePos - targetPos);
            double R = sphereR / cur_zoom;

            sphereCoord.X = (float)(R * Math.Sin(angleZ) * Math.Cos(angleXY));
            sphereCoord.Y = (float)(R * Math.Sin(angleZ) * Math.Sin(angleXY));
            sphereCoord.Z = (float)(R * Math.Cos(angleZ));

            eyePos = (targetPos + sphereCoord);


            eyeMatrix = Matrix4.LookAt(eyePos, targetPos, OZ * (float)Math.Pow(-1, times_changed_sign))
                * Matrix4.CreateScale(cur_zoom, cur_zoom, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref eyeMatrix);
        }
    }
    public enum RotationAxis { none = 0, oX = 1, oY = 2, oZ = 3 };
}
