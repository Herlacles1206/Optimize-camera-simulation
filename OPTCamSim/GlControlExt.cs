using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OPTCAMSim
{
    /// <summary>
    /// Extended GLControl: allows rotation, panning, turning, moving camera and zoom
    /// </summary>
    public partial class GlControlExt : GLControl
    {
        public OGLCamera oGLCamera;

        private Vector2 MousePos;
        private Vector2 MouseDelta;
        bool mouse_pressed = false;
        RotationAxis rAx = RotationAxis.none;
        public Matrix4 projection_;
        public bool loaded = false;
        public CSGPanel cSGPanel;
        public bool allow_invalidate = true;

        public GlControlExt()
        {
            InitializeComponent();
            this.MouseWheel += GlControlExt_MouseWheel;
        }

        public GlControlExt(OpenTK.Graphics.GraphicsMode m):base(m)
        {
            InitializeComponent();
            this.MouseWheel += GlControlExt_MouseWheel;
        }       

        private void GlControlExt_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouse_pressed = true;
            RefreshMousePos();
        }

        private void GlControlExt_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (mouse_pressed)
            {
                MouseState m = OpenTK.Input.Mouse.GetState();
                Vector2 newpos = new Vector2(m.X, m.Y);
                if (MousePos != Vector2.Zero)
                    MouseDelta = newpos - MousePos;
                if (MouseDelta != Vector2.Zero)
                {

                    if (rAx == RotationAxis.none)
                    {
                        rAx = (Math.Abs(MouseDelta.Y) > 2 && Math.Abs(MouseDelta.Y) > Math.Abs(MouseDelta.X) ? RotationAxis.oZ :
                             (Math.Abs(MouseDelta.X) > 2 && Math.Abs(MouseDelta.X) > Math.Abs(MouseDelta.Y) ?
                             ((m.LeftButton == OpenTK.Input.ButtonState.Pressed) ? RotationAxis.oX : RotationAxis.oY) : RotationAxis.none));
                    }
                    if (m.LeftButton == OpenTK.Input.ButtonState.Pressed)
                    {
                        if (rAx == RotationAxis.oZ)
                            oGLCamera.RotateCamera(0, -Math.Sign(MouseDelta.Y) * Math.PI * 2 / 360);
                        else if (rAx == RotationAxis.oX || rAx == RotationAxis.oY)
                            oGLCamera.RotateCamera(-Math.Sign(MouseDelta.X) * Math.PI * 2 / 360, 0);
                    }
                    if (m.RightButton == OpenTK.Input.ButtonState.Pressed)
                    {
                        if (rAx == RotationAxis.oZ)
                            oGLCamera.PanCamera(0, Math.Sign(MouseDelta.Y) * Math.PI * 2 / 360);
                        else if (rAx == RotationAxis.oX || rAx == RotationAxis.oY)
                            oGLCamera.PanCamera(Math.Sign(MouseDelta.X) * Math.PI * 2 / 360, 0);
                    }
                    if (m.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                    {
                        if (rAx != RotationAxis.none)
                        {
                            Keys mykey =
                                (rAx != RotationAxis.oZ) ? (Math.Sign(MouseDelta.X) == -1 ? Keys.D : Keys.A) :
                                (Math.Sign(MouseDelta.Y) == 1 ? Keys.W : Keys.S);
                            PreviewKeyDownEventArgs ea = new PreviewKeyDownEventArgs(mykey);

                            Vector3 move = GetMoveVector(ea);

                            oGLCamera.MoveCamera(move / 5);
                        }
                    }
                    this.Invalidate();
                }
                RefreshMousePos();
            }
        }
        private Vector3 GetMoveVector(PreviewKeyDownEventArgs e)
        {
            float step = 0.5f;
            if (cSGPanel != null && cSGPanel.Width > 0)
                step = cSGPanel.Width / (25f * oGLCamera.cur_zoom);

            return oGLCamera.GetMoveVector(step, e);
        }
        private void GlControlExt_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouse_pressed = false;
            RefreshMousePos();
            rAx = RotationAxis.none;
        }
        private void RefreshMousePos()
        {
            MouseState m = OpenTK.Input.Mouse.GetState();
            MousePos = new Vector2(m.X, m.Y);
        }
        private void GlControlExt_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
           if (!loaded) return;

            if (e.KeyCode == Keys.D1)
            {
                oGLCamera.RotateCamera(-Math.PI * 2 / 360, 0);
            }
            if (e.KeyCode == Keys.D2)
            {
                oGLCamera.RotateCamera(Math.PI * 2 / 360, 0);
            }
            if (e.KeyCode == Keys.D3)
            {
                oGLCamera.RotateCamera(0, -Math.PI * 2 / 360);
            }
            if (e.KeyCode == Keys.D4)
            {
                oGLCamera.RotateCamera(0, Math.PI * 2 / 360);
            }
            if (e.KeyCode == Keys.D5)
            {
                oGLCamera.PanCamera(-Math.PI * 2 / 360, 0);
            }
            if (e.KeyCode == Keys.D6)
            {
                oGLCamera.PanCamera(Math.PI * 2 / 360, 0);
            }
            if (e.KeyCode == Keys.D7)
            {
                oGLCamera.PanCamera(0, -Math.PI * 2 / 360);
            }
            if (e.KeyCode == Keys.D8)
            {
                oGLCamera.PanCamera(0, Math.PI * 2 / 360);
            }

            float step = 0.5f;
            if (cSGPanel!=null && cSGPanel.Width > 0)
                step = cSGPanel.Width / (25f * oGLCamera.cur_zoom);
            Vector3 move = GetMoveVector(e);
            if (move != Vector3.Zero)
            {
                oGLCamera.MoveCamera(move);
            }

            this.Invalidate();
        }
        private void GlControlExt_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            oGLCamera.Zoom(e);
            this.Invalidate();
        }

        private void GlControlExt_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            oGLCamera.Reset(cSGPanel);
            this.Invalidate();
        }

        public void SetTopView()
        {
            oGLCamera.TopView();
            this.Invalidate();
        }
    }
}
