using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Reflection;

namespace OPTCAMSim
{
    public class ShaderProgram
    {
        public int _shaderId;
        public int _fragObj;
        public int _vertexObj;
        /// <summary>
        /// Default shader constructor
        /// </summary>
        /// <param name="vs_res">vertex shader code file name</param>
        /// <param name="fs_res">fragment shader code file name</param>
        public ShaderProgram(string vs_res, string fs_res)
        {
            _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;

            GL.ShaderSource(_vertexObj,ReadResource(vs_res));
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            Console.Write(string.Format("triangle.vert compile: {0}", info));
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj, ReadResource(fs_res));
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
        }
        private string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public void Free()
        {
            GL.DeleteProgram(_shaderId);
            GL.DeleteShader(_vertexObj);
            GL.DeleteShader(_fragObj);
        }
        public void SetUniform3(string name, Vector3 value)
        {
            int pos = GL.GetUniformLocation(_shaderId, name);
            GL.ProgramUniform3(_shaderId, pos, ref value);
        }
        public void SetUniform1(string name, int value)
        {
            int pos = GL.GetUniformLocation(_shaderId, name);
            GL.ProgramUniform1(_shaderId, pos, value);
        }
        public void SetUniformMatrix4(string name, Matrix4 value)
        {
            int pos = GL.GetUniformLocation(_shaderId, name);
            GL.ProgramUniformMatrix4(_shaderId, pos, false, ref value);
        }
        public void Use()
        {
            GL.UseProgram(_shaderId);
        }
    }


}
