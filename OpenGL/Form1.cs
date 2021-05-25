using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Drawing.Imaging;

namespace OpenGL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int BasicProgramID;
        int BasicVertexShader;
        int BasicFragmentShader;
        int vaoHandle;
        int vbo_position;
        private bool InitShaders()
        {
            string glVersion = GL.GetString(StringName.Version);
            string glslVersion = GL.GetString(StringName.ShadingLanguageVersion);
     
            BasicProgramID = GL.CreateProgram();
            loadShader("..\\..\\Shaders\\basic.vert", ShaderType.VertexShader, BasicProgramID, out BasicVertexShader);
            loadShader("..\\..\\Shaders\\basic.frag", ShaderType.FragmentShader, BasicProgramID, out BasicFragmentShader);
            //Now that the shaders are added, the program needs to be linked.
            //Like C code, the code is first compiled, then linked, so that it goes
            //from human-readable code to the machine language needed.
            GL.LinkProgram(BasicProgramID);
            // Проверить успех компановки
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));

            /////////////////////////////////////////////////////

            float[] positionData = { -1f, -1f, 0.0f, 1f, -1f, 0.0f, 1f, 1f, 0.0f, -1f, 1f, 0.0f };
            float[] colorData = { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f };
            //Create and fill buffer objects;
            int[] vboHandlers = new int[2];
            GL.GenBuffers(2, vboHandlers);
            //Fill coordinate buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * positionData.Length), positionData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * colorData.Length), colorData, BufferUsageHint.StaticDraw);
            //Create vertex object
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[0]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[1]);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            /////////////////////////////////////////////////////////////////
            ///GL_QUAD;
            //Vector3[] vertdata = new Vector3[] {
            //    new Vector3(-1f, -1f, 0f),
            //    new Vector3( 1f, -1f, 0f),
            //    new Vector3( 1f, 1f, 0f),
            //    new Vector3(-1f, 1f, 0f) };
            //GL.GenBuffers(1, out vbo_position);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            //GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length *
            //Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            //int uniform_pos = GL.GetUniformLocation(BasicProgramID, "glPosition");
            //GL.Uniform3(uniform_pos, vertdata[0]);

            //GL.Uniform1(uniform_pos, -1);
            //GL.UseProgram(BasicProgramID);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            return true;
        }

        /// <summary>
        /// This creates a new shader (using a value from the ShaderType enum), loads code for it, compiles it, and adds it to our program.
        /// It also prints any errors it found to the console, which is really nice for when you make a mistake in a shader (it will also yell at you if you use deprecated code).
        /// </summary>
        /// <param name="filename">File to load the shader from</param>
        /// <param name="type">Type of shader to load</param>
        /// <param name="program">ID of the program to use the shader with</param>
        /// <param name="address">Address of the compiled shader</param>
        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private static bool Init()
        {
            GL.Enable(EnableCap.ColorMaterial);
            GL.ShadeModel(ShadingModel.Smooth);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            return true;
        }

        private static void Resize(int width, int height)
        {
            if (height == 0)
            {
                height = 1;
            }

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
            width / (float)height, 0.1f, 100.0f);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }


        private void Draw()
        {
            GL.ClearColor(Color.AliceBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();
            Matrix4 camera = Matrix4.LookAt(eye, target, up);
            GL.LoadMatrix(ref camera);

            GL.UseProgram(BasicProgramID);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            openGlControl.SwapBuffers();
            GL.UseProgram(0);

        }

        Vector3 eye = new Vector3(0,0,0);
        Vector3 target = new Vector3(0, 0, -1);
        Vector3 up = new Vector3(0, 1, 0);

        private double angle = 0;
        private void timer_Tick(object sender, EventArgs e)
        {
            angle += 5.0;
            openGlControl.Invalidate();
        }

        private void openGlControl_Paint(object sender, PaintEventArgs e)
        {
            Draw();
        }

        private void openGlControl_Load(object sender, EventArgs e)
        {
            Init();
            Resize(openGlControl.Width, openGlControl.Height);
            InitShaders();
        }

       
        private void openGlControl_Resize(object sender, EventArgs e)
        {
           Resize(openGlControl.Width, openGlControl.Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            openGlControl.Width = this.ClientRectangle.Width-24;
            openGlControl.Height = this.ClientRectangle.Height-24;
        }


        private void openGlControl_KeyDown(object sender, KeyEventArgs e)
        {
            float directionX = target.X - eye.X;
            float directionZ = target.Z - eye.Z;
            float length = (float)Math.Sqrt(directionX * directionX + directionZ * directionZ);
            directionX /= length;
            directionZ /= length;
            float delta = 0.1f;
            switch (e.KeyCode)
            {
                case Keys.W:
                    eye.X += directionX * delta;
                    eye.Z += directionZ * delta;
                    target.X += directionX * delta;
                    target.Z += directionZ * delta;
                    break;
                case Keys.S:
                    eye.X -= directionX * delta;
                    eye.Z -= directionZ * delta;
                    target.X -= directionX * delta;
                    target.Z -= directionZ * delta;
                    break;
                case Keys.A:
                    eye.X += directionZ * delta;
                    eye.Z -= directionX * delta;
                    break;
                case Keys.D:
                    eye.X -= directionZ * delta;
                    eye.Z += directionX * delta;
                    break;
                case Keys.Space:
                    eye.Y += 0.1f;
                    target.Y += 0.1f;
                    break;
                case Keys.Z:
                    eye.Y -= 0.1f;
                    target.Y -= 0.1f;
                    break;
            }
            openGlControl.Invalidate();
        }
        
        private int CreateTexture(string filename, bool IsRepeated = false, bool IsSmooth = true)
        {
            Bitmap bitmap = new Bitmap(filename);
            try
            {
                int TextureID = 0;
                GL.GenTextures(1, out TextureID);

                GL.BindTexture(TextureTarget.Texture2D, TextureID);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);

                // Setup filtering
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, IsRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, IsRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, IsSmooth ? Convert.ToInt32(TextureMagFilter.Linear) : Convert.ToInt32(TextureMagFilter.Nearest));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, IsSmooth ? Convert.ToInt32(TextureMinFilter.Linear) : Convert.ToInt32(TextureMinFilter.Nearest));

                return TextureID;
            }
            catch
            {
                return -1;
            }
        }

    }

}
