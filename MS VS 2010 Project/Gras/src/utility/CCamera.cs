using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Gras.src.utility
{
    enum EMoveType 
    {
        MOVE_JUMP_UP,
        MOVE_JUMP_DOWN,
        MOVE_LOOK_UP,
        MOVE_LOOK_DOWN,
        MOVE_LOOK_SIDE,
        MOVE_FORWARD,
        MOVE_BACK,
        MOVE_RIGHT,
        MOVE_LEFT
    };

    class CCamera
    {
        //For generating the View Matrix
        public Vector3 m_Position;
        public Vector3 m_LookAt;
        public Vector3 m_Direction;
        public Vector3 m_Up;

        //For Moving
        public float m_MoveSpeedMouse;
        public float m_MoveSpeedKeys;
        public float m_XAngle = 0.0f;
        public float m_YAngle = 0.0f;
        public float m_Framerate;
        public float m_MaxAngle;

        //For generating the Projection Matrix
        //near clip distance
        public float m_Near;
        //far clip distance
        public float m_Far;
        //Field of View
        public float m_Fov;
        //Width / Height
        public float m_aspectRatio;

        public CCamera(
            Vector3 position, 
            Vector3 lookat,
            Vector3 direction, 
            Vector3 up, 
            float moveSpeedMouse,
            float moveSpeedKeys,
            float maxAngle, 
            float near,
            float far, 
            float fov,
            float aspectRatio
        )
        {
            m_Position = position;
            m_LookAt = lookat;
            m_Direction = direction;
            m_Up = up;
            m_MoveSpeedMouse = moveSpeedMouse;
            m_MoveSpeedKeys = moveSpeedKeys;
            m_MaxAngle = maxAngle;
            m_Near = near;
            m_Far = far;
            m_Fov = fov;
            m_aspectRatio = aspectRatio;
        }

        //call in OnRender every frame
        public void UpdateView(out Matrix view)
        {
            //Line of sight x -z
		    m_LookAt.X = (float) Math.Sin((double) m_YAngle) + m_Position.X;
		    m_LookAt.Z = (float) Math.Cos((double) m_YAngle) + m_Position.Z;
		    		
		    //Height of sight
		    m_LookAt.Y = (float) Math.Sin((double) m_XAngle) + m_Position.Y;

		    //Sets the matrix
            view = Matrix.LookAtLH(m_Position, m_LookAt, m_Up); 
        }

        //call at init or when projective params are changed
        public void UpdateProj(out Matrix proj)
        {
            proj = Matrix.PerspectiveFovLH(m_Fov, m_aspectRatio, m_Near, m_Far);
        }

        public void Update(out Matrix proj, out Matrix view)
        {
            m_Direction = Vector3.Normalize(m_LookAt - m_Position);

            view = Matrix.LookAtLH(m_Position, m_LookAt, m_Up);
            proj = Matrix.PerspectiveFovLH(m_Fov, m_aspectRatio, m_Near, m_Far);
        }

        public void AddToCamera(float Strafe, float UpDown, float ForBack, out Matrix m_proj, out Matrix m_view)
        {
            Vector3 Direction = m_Direction;
            Vector3 Ortho = new Vector3(-Direction.Z, 0.0f, Direction.X);
            Ortho = Ortho * Strafe;

            Direction = Direction * ForBack;

            Vector3 VecUpDown = new Vector3(0.0f, 1.0f * UpDown, 0.0f);

            m_LookAt = m_LookAt + Direction + VecUpDown + Ortho;
            m_Position = m_Position + Direction + VecUpDown + Ortho;

            Update(out m_proj, out m_view);
        }

        public void RotateAroundPosition(float x, float y, out Matrix m_proj, out Matrix m_view)
        {
            Matrix matRotate;

            //Rotation Left/Right
            m_LookAt = m_LookAt - m_Position;
            matRotate = Matrix.RotationAxis(new Vector3(0.0f, 1.0f, 0.0f), x);
            Vector4 temp = Vector3.Transform(m_LookAt, matRotate);
            m_LookAt.X = temp.X;
            m_LookAt.Y = temp.Y;
            m_LookAt.Z = temp.Z;
            m_LookAt = m_LookAt + m_Position;

            m_Direction = Vector3.Normalize(m_LookAt - m_Position);

            //Rotation Up/Down
            Vector3 Ortho = new Vector3(-m_Direction.Z, 0.0f, m_Direction.X);

            m_LookAt = m_LookAt - m_Position;
            matRotate = Matrix.RotationAxis(Ortho, y);

            temp = Vector3.Transform(m_LookAt, matRotate);
            m_LookAt.X = temp.X;
            m_LookAt.Y = temp.Y;
            m_LookAt.Z = temp.Z;
            m_LookAt = m_LookAt + m_Position;

            Update(out m_proj, out m_view);
        }

        //call with OnRenderBegin
        public void moveCamera(float mouseMove, EMoveType moveType)
        {
            //Division by 0
            if (m_Framerate == 0)
                m_Framerate = 25;

            //Jump up
            if (moveType == EMoveType.MOVE_JUMP_UP)
                m_Position.Y += m_MoveSpeedKeys;

            //Jump down
            if (moveType == EMoveType.MOVE_JUMP_DOWN)
                m_Position.Y -= m_MoveSpeedKeys;

            //Sets it true
            bool bAngleUP = true;

            //If the max angle exist
            if (m_MaxAngle != 0.0f)
            {
                //Sets it false
                bAngleUP = false;

                //Sets it true, if it's under the max angle
                if ((moveType == EMoveType.MOVE_LOOK_UP) && (m_XAngle > -m_MaxAngle))
                    bAngleUP = true;
                else if ((moveType == EMoveType.MOVE_LOOK_DOWN) && (m_XAngle < m_MaxAngle))
                    bAngleUP = true;
            }

            //Angle Up
            if ((moveType == EMoveType.MOVE_LOOK_UP) && bAngleUP)
                m_XAngle -= mouseMove * m_MoveSpeedMouse / m_Framerate;

            //Angle Down
            if ((moveType == EMoveType.MOVE_LOOK_DOWN) && bAngleUP)
                m_XAngle -= mouseMove * m_MoveSpeedMouse / m_Framerate;

            //Angle Left/Right
            if (moveType == EMoveType.MOVE_LOOK_SIDE)
                m_YAngle += mouseMove * m_MoveSpeedMouse / m_Framerate;



            //Move forward
            if (moveType == EMoveType.MOVE_FORWARD)
            {
                m_Position.X += m_MoveSpeedKeys * (float)Math.Sin(m_YAngle) / m_Framerate;
                m_Position.Z += m_MoveSpeedKeys * (float)Math.Cos(m_YAngle) / m_Framerate;
            }

            //Move backward
            if (moveType == EMoveType.MOVE_BACK)
            {
                m_Position.X -= m_MoveSpeedKeys * (float)Math.Sin(m_YAngle) / m_Framerate;
                m_Position.Z -= m_MoveSpeedKeys * (float)Math.Cos(m_YAngle) / m_Framerate;
            }

            //Move right
            if (moveType == EMoveType.MOVE_RIGHT)
            {
                m_Position.X += m_MoveSpeedKeys * (float)Math.Sin(Math.PI / 2.0 + m_YAngle) / m_Framerate;
                m_Position.Z += m_MoveSpeedKeys * (float)Math.Cos(Math.PI / 2.0 + m_YAngle) / m_Framerate;
            }

            //Move left
            if (moveType == EMoveType.MOVE_LEFT)
            {
                m_Position.X -= m_MoveSpeedKeys * (float)Math.Sin(Math.PI / 2.0f + m_YAngle) / m_Framerate;
                m_Position.Z -= m_MoveSpeedKeys * (float) Math.Cos(Math.PI / 2.0f + m_YAngle) / m_Framerate;
            }
        }

    }
}
