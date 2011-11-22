using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;

namespace RealtimeGrass.Utility
{
    class Camera
    {
        //For generating the View Matrix
        public Vector3 m_Position;
        public Vector3 m_LookAt;
        public Vector3 m_Direction;
        public Vector3 m_Up;

        //For Moving
        public float m_MoveSpeedMouse;
        public float m_MoveSpeedKeys;
        public bool isSlowMoving;

        //For generating the Projection Matrix
        //near clip distance
        public float m_Near;
        //far clip distance
        public float m_Far;
        //Field of View
        public float m_Fov;
        //Width / Height
        public float m_aspectRatio;

        public Camera(
            Vector3 position, 
            Vector3 lookat,
            Vector3 direction, 
            Vector3 up, 
            float moveSpeedMouse,
            float moveSpeedKeys,
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
            m_Near = near;
            m_Far = far;
            m_Fov = fov;
            m_aspectRatio = aspectRatio;
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
            //For taking speed into account
            Strafe *= m_MoveSpeedKeys;
            UpDown *= m_MoveSpeedKeys;
            ForBack *= m_MoveSpeedKeys;

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

            //For taking speed into account
            x *= m_MoveSpeedMouse;
            y *= m_MoveSpeedMouse;

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

            if (
                y > 0.0f && Vector3.Dot(Vector3.UnitY, m_Direction) < 0.99f ||
                y < 0.0f && Vector3.Dot(-Vector3.UnitY, m_Direction) < 0.99f
            )
            {
                m_LookAt = m_LookAt - m_Position;
                matRotate = Matrix.RotationAxis(Ortho, y);

                temp = Vector3.Transform(m_LookAt, matRotate);
                m_LookAt.X = temp.X;
                m_LookAt.Y = temp.Y;
                m_LookAt.Z = temp.Z;
                m_LookAt = m_LookAt + m_Position;
            }

            Update(out m_proj, out m_view);
        }

        public void LookAtPosition(float x, float y, out Matrix m_proj, out Matrix m_view)
        {
            Matrix matRotate;

            x *= m_MoveSpeedMouse;
            y *= m_MoveSpeedMouse;

            //Rotation Left/Right
            m_LookAt = m_LookAt - m_Position;
            matRotate = Matrix.RotationAxis(new Vector3(0.0f, 1.0f, 0.0f), x);
            Vector4 temp = Vector3.Transform(m_Position, matRotate);
            m_Position.X = temp.X;
            m_Position.Y = temp.Y;
            m_Position.Z = temp.Z;

            m_Direction = Vector3.Normalize(m_LookAt - m_Position);

            //Rotation Up/Down
            Vector3 Ortho = new Vector3(-m_Direction.Z, 0.0f, m_Direction.X);

            m_LookAt = m_LookAt - m_Position;
            matRotate = Matrix.RotationAxis(Ortho, y);

            temp = Vector3.Transform(m_Position, matRotate);
            m_Position.X = temp.X;
            m_Position.Y = temp.Y;
            m_Position.Z = temp.Z;

            Update(out m_proj, out m_view);
        }
    }
}
