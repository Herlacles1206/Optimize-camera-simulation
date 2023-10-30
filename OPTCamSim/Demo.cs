using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Linq;

namespace OPTCAMSim
{

    public class TDemoWindow : ExtendedGameWindow
    {
        int _shaderId;
        int _vao;
        int _glbuf;
        int _fragObj;
        int _vertexObj;
        float[] _vertices;

        int FontTextureID;
        Vector3 eyePos = new Vector3(4, 3, 3);

        public List<Net3dBool.Solid> MeshList = new List<Net3dBool.Solid> { };
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            float[] mat_diffuse = { 1.0f, 0.0f, 0.0f };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, mat_diffuse);
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 1.0f, 1.0f, 0.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 10.0f, 10.0f, 10.0f, 1.0f });

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);

            InitShader();
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            FreeShader(); 
        }
        private void InitShader()
        {
            _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;


            GL.ShaderSource(_vertexObj,
                 @"
#version 420 core
uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

layout(location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;
layout(location = 3) in float aTexInd;

out vec2 texCoord;
out vec3 FragPos;  
out vec3 Normal;
out float TexInd;
void main()
{
    FragPos = vec3(model * vec4(aPos, 1.0));
    Normal = aNormal;  
    texCoord = aTexCoord;
    TexInd = aTexInd;
    gl_Position = projection * view * vec4(FragPos, 1.0);
}"

                );
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            Console.Write(string.Format("triangle.vert compile: {0}", info));
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj,
@"
#version 420 core
out vec4 FragColor;

in vec3 FragPos;  
in vec3 Normal; 
in vec2 texCoord;
in float TexInd;

uniform vec3 lightPos; 
uniform vec3 lightColor;
uniform vec3 objectColr;
uniform vec3 viewPos;

uniform sampler2D texture0;

void main()
{    
   // ambient
    float ambientStrength = 0.5;
    vec3 ambient = ambientStrength * lightColor;
  	
    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;
            
    // specular
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;         
   
    vec3 result = (ambient + diffuse + specular) * objectColr;
    if(TexInd==1)
        FragColor = texture(texture0, texCoord) * vec4( result,1);
    else
        FragColor = vec4( result,1)  ;
}
");
            GL.CompileShader(_fragObj);
            info = GL.GetShaderInfoLog(_fragObj);
            Console.Write(string.Format("triangle.frag compile: {0}", info));
            GL.GetShader(_fragObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            _shaderId = GL.CreateProgram();
            GL.AttachShader(_shaderId, _fragObj);
            GL.AttachShader(_shaderId, _vertexObj);
            GL.LinkProgram(_shaderId);
            Console.Write(string.Format("link program: {0}", GL.GetProgramInfoLog(_shaderId)));
            GL.UseProgram(_shaderId);
            Console.Write(string.Format("use program: {0}", GL.GetProgramInfoLog(_shaderId)));

            _vao = GL.GenVertexArray();
            _glbuf = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glbuf);

            string Text = "This is a test" + Environment.NewLine + "With two lines";
            Font font = new Font("Arial", 14);
            Size s = TextRenderer.MeasureText(Text, font);
            int bitmapWidth = s.Width;
            int bitmapHeight = s.Height;

            Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bitmap))
            {
                //fill background
                using (var path = new GraphicsPath())
                { 
                    path.AddPolygon(new PointF[] { new Point(0, 0) , new Point(bitmapWidth, 0), new Point(bitmapWidth, bitmapHeight), new Point(0, bitmapHeight)});

                    using (var brush = new SolidBrush(Color.White))
                    {
                       g.FillPath(brush, path);
                    }
                }
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(Text, font , Brushes.Black, 0, 0);
            }
            

            GL.GenTextures(1,out FontTextureID);
            GL.BindTexture(TextureTarget.Texture2D, FontTextureID);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            //Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // normal attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            //Texture u v
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            //Texture index
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(3);

            //We use an Identity matrix for the model
            Matrix4 _model = Matrix4.Identity;

            Matrix4 _projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 0.1f, 150.0f);

            int projLoc = GL.GetUniformLocation(_shaderId, "projection");
            GL.ProgramUniformMatrix4(_shaderId, projLoc, false, ref _projection);

            int modelLoc = GL.GetUniformLocation(_shaderId, "model");
            GL.ProgramUniformMatrix4(_shaderId, modelLoc, false, ref _model);

            //Light
            int lightpos = GL.GetUniformLocation(_shaderId, "lightPos");
            Vector3 LightPos = new Vector3(15f, 13f,20f);
            GL.ProgramUniform3(_shaderId, lightpos, ref LightPos);

            int lightColorLoc = GL.GetUniformLocation(_shaderId, "lightColor");
            Vector3 lc = new Vector3(1.0f, 1.0f, 1.0f);
            GL.ProgramUniform3(_shaderId, lightColorLoc, ref lc);

            int objectColorLoc = GL.GetUniformLocation(_shaderId, "objectColr");
            Vector3 oc = new Vector3(1.0f, 0, 0);
            GL.ProgramUniform3(_shaderId, objectColorLoc, ref oc);

        }


        void FreeShader()
        {
            GL.DeleteProgram(_shaderId);
            GL.DeleteShader(_vertexObj);
            GL.DeleteShader(_fragObj);
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_glbuf);
        }




        public override void CreateMesh()
        {
            Console.Write("Generate mesh...");

            var box = new Net3dBool.Solid(Net3dBool.DefaultCoordinates.DEFAULT_BOX_VERTICES, Net3dBool.DefaultCoordinates.DEFAULT_BOX_COORDINATES);
            
            string err = "";
            Tool MyTool = Tool.GetToolFromFile(Application.StartupPath + "\\UDT\\T5.txt",  ref err);
            var box2 = MyTool.ToolObj;
            box2.Scale(0.4, 0.4, 0.2);
            box2.Translate(0.1, 0.1,0.1);
            int m = 20;
            for (int k = 0; k < m; k++)
            {
                Net3dBool.Mesh m1 = new Net3dBool.Mesh(box.GetVertices().ToList(), box.GetIndices().ToList());
                Net3dBool.Mesh m2 = new Net3dBool.Mesh(box2.GetVertices().ToList(), box2.GetIndices().ToList());

                System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
                time.Start();
                Net3dBool.newCSG nCSG = new Net3dBool.newCSG(m1, m2);
                box = nCSG.GetDifference();
                //box.RoundVerticesCoords(7);
                time.Stop();
                Console.WriteLine("Elapsed time: " + time.ElapsedMilliseconds / 1000f + " s");
                box2.Translate(0.01, 0, 0);
            }

            MeshList.Add(box);
        }

        public override void RenderMesh()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            List<float> verts_ar = new List<float> { };
            for (int q = 0; q < MeshList.Count; q++)
            {
                Net3dBool.Solid Mesh = MeshList[q];
                Color col = Mesh.color;

                var verts = Mesh.GetVertices();
                int[] ind = Mesh.GetIndices();

                for (var i = 0; i < ind.Length; i = i + 3)
                {
                    Vector3 n = GetNormal(verts[ind[i]], verts[ind[i + 1]], verts[ind[i + 2]]);
                    verts_ar.AddRange(new List<float>{(float)verts[ind[i]].X, (float)verts[ind[i]].Y, (float)verts[ind[i]].Z, n.X, n.Y, n.Z, i%2,i%2,0,// i>29?1:0,
                                        (float)verts[ind[i + 1]].X, (float)verts[ind[i + 1]].Y, (float)verts[ind[i + 1]].Z, n.X, n.Y, n.Z, i%2,1-i%2, 0,//i>29?1:0,
                                        (float)verts[ind[i + 2]].X, (float)verts[ind[i + 2]].Y, (float)verts[ind[i + 2]].Z, n.X, n.Y, n.Z , 1-i%2,1-i%2, 0//i>29?1:0
                    }
                        );
                }
            }
            _vertices = verts_ar.ToArray();

            GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _vertices.Length), _vertices, BufferUsageHint.StaticDraw);
            
            GL.BindVertexArray(0);

            // Attempt to build our model view projection matrix


            int viewLoc = GL.GetUniformLocation(_shaderId, "view");
            GL.ProgramUniformMatrix4(_shaderId, viewLoc, false, ref CameraMatrix);

            int viewpos = GL.GetUniformLocation(_shaderId, "viewPos");
            GL.ProgramUniform3(_shaderId, viewpos, ref CameraLocation);

            GL.BindTexture(TextureTarget.Texture2D, FontTextureID);
            GL.BindVertexArray(_vao);
           
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 2);
            GL.BindVertexArray(0); 
        }
        public Vector3 GetNormal(Net3dBool.Vector3 v1, Net3dBool.Vector3 v2, Net3dBool.Vector3 v3)
        {
            Net3dBool.Vector3 firstvec = v2 - v1;
            Net3dBool.Vector3 secondvec = v1 - v3;
            Net3dBool.Vector3 normal = Net3dBool.Vector3.Cross(firstvec, secondvec);
            normal.Normalize();
            return new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z);
        }

    }

    public class Vertex
    {
        public Vector3 pos;
        public Vector3 Normal;
        public Color color;
        public static int SizeInBytes { get { return 2 * Vector3.SizeInBytes + 4; } }
        
    }
}

