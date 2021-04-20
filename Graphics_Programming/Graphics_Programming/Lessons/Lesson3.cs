using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GraphicsProgramming.Lessons
{
    class Lesson3 : Lesson
    {
        Model sphere, Cube;
        Texture2D day, night, clouds, moon;
        TextureCube sky;

        private Effect myEffect;
        Vector3 LightPosition = Vector3.Right * 2 + Vector3.Up * 2 + Vector3.Backward * 2;

        float scrol = 40;
        float yaw, pitch;
        int prevX, prevY;
        public override void Initialize()
        {
            //scrol += 20;
        }
        public override void Update(GameTime gameTime)
        {
            MouseState mState = Mouse.GetState();
            scrol = mState.ScrollWheelValue;

            if (mState.LeftButton == ButtonState.Pressed)
            {
                yaw -= (mState.X - prevX) * 0.01f;
                pitch -= (mState.Y - prevY) * 0.01f;

                pitch = MathF.Min(MathF.Max(pitch, -MathF.PI * 0.45f), MathF.PI * 0.45f);
            }

            prevX = mState.X;
            prevY = mState.Y;
        }
        public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            myEffect = Content.Load<Effect>("HomeWork3");

            day = Content.Load<Texture2D>("day");
            night = Content.Load<Texture2D>("night");
            clouds = Content.Load<Texture2D>("clouds");
            moon = Content.Load<Texture2D>("2k_moon");
            sky = Content.Load<TextureCube>("sky_cube");

            Sphere = Content.Load<Model>("uv_sphere");

            foreach (ModelMesh mesh in Sphere.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = myEffect;
                }
            }

            Cube = Content.Load<Model>("cube");

            foreach (ModelMesh mesh2 in Cube.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh2.MeshParts)
                {
                    meshPart.Effect = myEffect;
                }
            }
        }


        public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            //LightPosition = new Vector3(MathF.Cos(time), 0, MathF.Sin(time)) * 200;
            LightPosition = Vector3.Left * 200;

            Vector3 cameraPos = -Vector3.Forward * scrol / 100;// + Vector3.Up * 5;
            cameraPos = Vector3.Transform(cameraPos, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0));

            Matrix World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix View = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);

            myEffect.Parameters["World"].SetValue(World);
            myEffect.Parameters["View"].SetValue(View);
            myEffect.Parameters["Projection"].SetValue(Matrix.CreatePerspectiveFieldOfView((MathF.PI / 180f) * 25f, device.Viewport.AspectRatio, 0.001f, 1000f));

            myEffect.Parameters["Time"].SetValue(time);
            myEffect.Parameters["LightPosition"].SetValue(LightPosition);
            myEffect.Parameters["CameraPosition"].SetValue(cameraPos);

            myEffect.Parameters["DayTex"].SetValue(day);
            myEffect.Parameters["NightTex"].SetValue(night);
            myEffect.Parameters["CloudsTex"].SetValue(clouds);
            myEffect.Parameters["MoonTex"].SetValue(moon);
            myEffect.Parameters["SkyTex"].SetValue(sky);

            device.Clear(Color.Black);
            myEffect.CurrentTechnique.Passes[0].Apply();

            myEffect.CurrentTechnique = myEffect.Techniques["Sky"];
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            RenderModel(Cube, Matrix.CreateTranslation(cameraPos));

            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;

            myEffect.CurrentTechnique = myEffect.Techniques["Earth"];
            RenderModel(Sphere, World * Matrix.CreateScale(0.01f) * Matrix.CreateRotationZ(time / 4) * Matrix.CreateRotationY(MathF.PI / 180 * 23) * World);
            myEffect.CurrentTechnique = myEffect.Techniques["Moon"];
            RenderModel(Sphere, Matrix.CreateTranslation(Vector3.Down * 8) * Matrix.CreateScale(0.0033f) * Matrix.CreateRotationZ(time) * World);
        }

        void RenderModel(Model m, Matrix parentMatrix)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);
            myEffect.CurrentTechnique.Passes[0].Apply();

            foreach (ModelMesh mesh in m.Meshes)
            {
                myEffect.Parameters["World"].SetValue(parentMatrix * transforms[mesh.ParentBone.Index]);
                mesh.Draw();
            }
        }
    }
}
