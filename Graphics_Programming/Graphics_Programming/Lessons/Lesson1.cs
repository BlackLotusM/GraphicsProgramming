using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphicsProgramming.Lessons
{
    class Lesson1 : Lesson
    {
		VertexPositionColor[] vertices =
	   {
            //front 
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), Color.LightBlue),
			new VertexPositionColor(new Vector3(0.5f, -0.5f,0.5f), Color.LightBlue),
			new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), Color.LightBlue),
			new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), Color.LightBlue),

            //back
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), Color.LightCoral),
			new VertexPositionColor(new Vector3(0.5f, -0.5f,-0.5f), Color.LightCoral),
			new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), Color.LightCoral),
			new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), Color.LightCoral),
		};

		int[] indices =
		{
            //front
            0,1,2,
			0,3,1,

            //back
            6,5,4,
			5,7,4,

            //right
            3,5,1,
			3,7,5,

            //left
            2,6,0,
			6,4,0,

            //under
            2,5,6,
			2,1,5,

            //top
            4,7,0,
			7,3,0,
		};

		BasicEffect effect;

		public override void LoadContent(ContentManager Content, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
		{
			effect = new BasicEffect(graphics.GraphicsDevice);
		}

		public override void Draw(GameTime gameTime, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
		{
			GraphicsDevice device = graphics.GraphicsDevice;
			device.Clear(Color.BlanchedAlmond);
			effect.VertexColorEnabled = true;
			effect.CurrentTechnique.Passes[0].Apply();

			effect.World = Matrix.Identity * Matrix.CreateRotationX((float)gameTime.TotalGameTime.TotalSeconds) * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds);
			effect.View = Matrix.CreateLookAt(-Vector3.Forward * 2, Vector3.Zero, Vector3.Up);
			effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 180 * 65f, device.Viewport.AspectRatio, 0.1f, 100f);

			
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
			}
		}
	}
}
