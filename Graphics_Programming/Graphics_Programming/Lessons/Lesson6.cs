using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraphicsProgramming
{
	class Lesson6 : Lesson
    {
        private Effect effect, postfx;
        private Texture2D heightmap, dirt, water, grass, rock, snow, plant, waterNormal;
        private TextureCube sky;
        private Model cube, sphere;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vert : IVertexType
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector3 binormal;
            public Vector3 tangent;
            public Vector2 texture;
            static readonly VertexDeclaration _vertexDeclaration =
                new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                    new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                    new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
            VertexDeclaration IVertexType.VertexDeclaration =>
                _vertexDeclaration;
            public Vert(Vector3 Position, Vector3 Normal, Vector3 Binormal, Vector3 Tangent, Vector2 Texture)
            {
                position = Position;
                normal = Normal;
                binormal = Binormal;
                tangent = Tangent;
                texture = Texture;
            }
        }

        private Vert[] vertices;
        private int[] indices;
        private int mouseX, mouseY;
        Vector3 cameraPos = Vector3.Up * 500 - Vector3.Forward * 1400 + Vector3.Right * 512;
        Quaternion cameraRotation = Quaternion.Identity;
        float yaw, pitch;
        RenderTarget2D rt1, rt2;
        Texture2D backBuffer;
        Color[] backBufferPixels;

        public override void Initialize()
        {
            mouseX = Mouse.GetState().X;
            mouseY = Mouse.GetState().Y;
        }

        public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {

            effect = Content.Load<Effect>("HomeWork56");
            postfx = Content.Load<Effect>("PostFXLive");

            heightmap = Content.Load<Texture2D>("heightmap");
            dirt = Content.Load<Texture2D>("dirt_diff");
            water = Content.Load<Texture2D>("water");
            grass = Content.Load<Texture2D>("grass");
            rock = Content.Load<Texture2D>("rock");
            snow = Content.Load<Texture2D>("snow");
            waterNormal = Content.Load<Texture2D>("waternormal");
            //plant = Content.Load<Texture2D>("grass_transparent");

            sphere = Content.Load<Model>("uv_sphere");
            foreach (ModelMesh mesh in sphere.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }

            cube = Content.Load<Model>("cube");
            foreach (ModelMesh mesh in cube.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }

            GeneratePlane(8.0f, 512f);

            rt1 = new RenderTarget2D(graphics.GraphicsDevice,
                graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight,
                false, // no mipmaps
                graphics.PreferredBackBufferFormat, graphics.PreferredDepthStencilFormat
                );

            rt2 = new RenderTarget2D(graphics.GraphicsDevice,
                graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight,
                false, // no mipmaps
                graphics.PreferredBackBufferFormat, graphics.PreferredDepthStencilFormat
                );

            backBuffer = new Texture2D(graphics.GraphicsDevice,
                graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight,
                false, // no mipmaps
                graphics.PreferredBackBufferFormat
                );

            backBufferPixels = new Color[graphics.PreferredBackBufferWidth * graphics.PreferredBackBufferHeight];
        }

        private void GeneratePlane(float gridSize = 8.0f, float height = 128f)
        {
            // Get pixels
            Color[] pixels = new Color[heightmap.Width * heightmap.Height];
            heightmap.GetData<Color>(pixels);

            // Generate vertices & indices
            vertices = new Vert[pixels.Length];
            indices = new int[(heightmap.Width - 1) * (heightmap.Height - 1) * 6];

            // for loops
            for (int y = 0; y < heightmap.Height; y++)
            {
                for (int x = 0; x < heightmap.Width; x++)
                {
                    int index = y * heightmap.Width + x;

                    float r = pixels[index].R / 255f;

                    //smooth if not at the edges
                    if (y < heightmap.Height - 2 && x < heightmap.Width - 2)
                    {
                        r += pixels[index + 1].R / 255f;
                        r += pixels[index + heightmap.Width].R / 255f;
                        r += pixels[index + heightmap.Width + 1].R / 255f;
                        r *= 0.25f;
                    }

                    // add vertex for current pixel
                    vertices[index] = new Vert(new Vector3(gridSize * x, r * height, gridSize * y),
                    Vector3.Up, Vector3.Up, Vector3.Up, // normals
                    new Vector2(x / (float)heightmap.Width, y / (float)heightmap.Height)
                    );

                    // if not at edge
                    if (y < heightmap.Height - 2 && x < heightmap.Width - 2)
                    {
                        // add indices for TWO triangles (bottom-right)
                        int right = y * heightmap.Width + (x + 1);                  // index + 1
                        int bottom = (y + 1) * heightmap.Width + x;                 // index + width
                        int bottomRight = (y + 1) * heightmap.Width + (x + 1);      // index + width + 1

                        // triangle 1 bottom left
                        indices[index * 6 + 0] = index;
                        indices[index * 6 + 1] = bottomRight;
                        indices[index * 6 + 2] = bottom;

                        // triangle 2 top right
                        indices[index * 6 + 3] = index;
                        indices[index * 6 + 4] = right;
                        indices[index * 6 + 5] = bottomRight;
                    }
                }
            }

            // Calculate normals
            for (int y = 0; y < heightmap.Height - 1; y++)
            {
                for (int x = 0; x < heightmap.Width - 1; x++)
                {
                    int index = y * heightmap.Width + x;

                    int right = y * heightmap.Width + (x + 1);                  // index + 1
                    int bottom = (y + 1) * heightmap.Width + x;                 // index + width

                    // vector pointing right
                    Vector3 vr = Vector3.Normalize(vertices[right].position - vertices[index].position);
                    // vector pointing down
                    Vector3 vd = Vector3.Normalize(vertices[bottom].position - vertices[index].position);

                    vertices[index].normal = Vector3.Cross(vr, vd);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float speed = 100;
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                speed *= 2;
            }
            if (keyState.IsKeyDown(Keys.W))
            {
                cameraPos += delta * speed * Vector3.Transform(Vector3.Forward, cameraRotation);
            }
            else if (keyState.IsKeyDown(Keys.S))
            {
                cameraPos -= delta * speed * Vector3.Transform(Vector3.Forward, cameraRotation);
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                cameraPos += delta * speed * Vector3.Transform(Vector3.Left, cameraRotation);
            }
            else if (keyState.IsKeyDown(Keys.D))
            {
                cameraPos += delta * speed * Vector3.Transform(Vector3.Right, cameraRotation);
            }
            if (keyState.IsKeyDown(Keys.Space))
            {
                cameraPos += delta * speed * Vector3.Transform(Vector3.Up, cameraRotation);
            }
            else if (keyState.IsKeyDown(Keys.LeftControl))
            {
                cameraPos += delta * speed * Vector3.Transform(Vector3.Down, cameraRotation);
            }

            MouseState mState = Mouse.GetState();
            int deltaX = mState.X - mouseX;
            int deltaY = mState.Y - mouseY;

            float sensitivity = 0.01f;

            yaw -= deltaX * sensitivity;
            pitch -= deltaY * sensitivity;

            pitch = Math.Clamp(pitch, -MathF.PI * .5f, MathF.PI * .5f);

            cameraRotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

            mouseX = mState.X;
            mouseY = mState.Y;

            if (mState.RightButton == ButtonState.Pressed)
            {
                yaw = 0; pitch = 0;
            }
        }

        public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            //device.SetRenderTarget(rt);

            device.Clear(Color.Black);
            float r = (float)gameTime.TotalGameTime.TotalSeconds;

            // Build & Set Matrices
            Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix View = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(Vector3.Forward, cameraRotation), Vector3.Transform(Vector3.Up, cameraRotation));
            Matrix Projection = Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 65f, device.Viewport.AspectRatio, 0.01f, 2000f);

            effect.Parameters["World"].SetValue(World);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);

            // Lighting Parameters
            effect.Parameters["LightDirection"].SetValue(Vector3.Normalize(Vector3.Down + Vector3.Right * 2));
            effect.Parameters["Ambient"].SetValue(new Vector3(.25f, .20f, .15f));
            effect.Parameters["CameraPosition"].SetValue(cameraPos);

            // Textures
            effect.Parameters["DirtTex"].SetValue(dirt);
            effect.Parameters["WaterTex"].SetValue(water);
            effect.Parameters["GrassTex"].SetValue(grass);
            effect.Parameters["RockTex"].SetValue(rock);
            effect.Parameters["SnowTex"].SetValue(snow);

            effect.Parameters["Time"].SetValue(r);
            postfx.Parameters["Time"].SetValue(r);
            // Render Sky
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            effect.CurrentTechnique = effect.Techniques["SkyBox"];
            RenderModel(cube, Matrix.CreateTranslation(cameraPos));

            // Render Terrain
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
            effect.CurrentTechnique = effect.Techniques["Terrain"];
            effect.Parameters["World"].SetValue(World);

            effect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);

            // TODO: Copy backbuffer to Texture2D
            device.GetBackBufferData<Color>(backBufferPixels);
            backBuffer.SetData<Color>(backBufferPixels);

            // Select new technique
            //device.BlendState = BlendState.AlphaBlend;
            effect.CurrentTechnique = effect.Techniques["HeatDistort"];

            effect.Parameters["GrassTex"].SetValue(backBuffer);
            effect.Parameters["WaterTex"].SetValue(waterNormal);

            // Render Inside
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullNone;
            RenderModel(sphere, World * Matrix.CreateTranslation(Vector3.Right * 800 - Vector3.Forward * 2000 + Vector3.Up * 300));

            // Render Outside
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.BlendState = BlendState.Opaque;


            ////////////////////////////
            /////POSTPROCESSING////////
            ///////////////////////////
            device.GetBackBufferData<Color>(backBufferPixels);
            backBuffer.SetData<Color>(backBufferPixels);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, postfx);

            postfx.CurrentTechnique = postfx.Techniques["Invert"];
            device.SetRenderTarget(rt1);
            spriteBatch.Draw(backBuffer, Vector2.Zero, Color.White);

            postfx.CurrentTechnique = postfx.Techniques["ChromaticAberration"];
            device.SetRenderTarget(rt2);
            spriteBatch.Draw(rt1, Vector2.Zero, Color.White);

            postfx.CurrentTechnique = postfx.Techniques["Invert"];
            device.SetRenderTarget(rt1);
            spriteBatch.Draw(rt2, Vector2.Zero, Color.White);

            postfx.CurrentTechnique = postfx.Techniques["Vignette"];
            device.SetRenderTarget(rt2);
            spriteBatch.Draw(rt1, Vector2.Zero, Color.White);

            postfx.CurrentTechnique = postfx.Techniques["Invert"];
            device.SetRenderTarget(rt1);
            spriteBatch.Draw(rt2, Vector2.Zero, Color.White);

            //Do effect here
            spriteBatch.End();

            device.SetRenderTarget(null);

            spriteBatch.Begin();
            // coppy last written rendertarget to screen
            spriteBatch.Draw(rt2, Vector2.Zero, Color.White);
            spriteBatch.End();
            ////////////////////////////
            /////End POSTPROCESSING////////
            ///////////////////////////
        }

        void RenderModel(Model m, Matrix parentMatrix)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);
            effect.CurrentTechnique.Passes[0].Apply();
            foreach (ModelMesh mesh in m.Meshes)
            {
                effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * parentMatrix);
                mesh.Draw();
            }
        }
    }
}
