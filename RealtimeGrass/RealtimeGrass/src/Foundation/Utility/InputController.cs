using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

using SlimDX;
using SlimDX.DirectInput;

namespace RealtimeGrass.Utility
{
    class InputController
    {
        Keyboard m_keyboard;
        Mouse m_mouse;

        // make sure that DirectInput has been initialized
        DirectInput m_dinput = new DirectInput();

        // build up cooperative flags
        CooperativeLevel m_mouseCooperativeLevel;
        CooperativeLevel m_keyboardCooperativeLevel;

        public InputController(Form form)
        {
            m_mouseCooperativeLevel = CooperativeLevel.Foreground | CooperativeLevel.Exclusive;
            m_keyboardCooperativeLevel = CooperativeLevel.Foreground | CooperativeLevel.Exclusive;

            try
            {
                m_keyboard = new Keyboard(m_dinput);
                m_keyboard.SetCooperativeLevel(form, m_keyboardCooperativeLevel);

                m_mouse = new Mouse(m_dinput);
                m_mouse.SetCooperativeLevel(form, m_mouseCooperativeLevel);
            }
            catch (DirectInputException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            // acquire the device
            m_keyboard.Acquire();

            // acquire the device
            m_mouse.Acquire();
        }
        
        public KeyboardState ReadKeyboard()
        {
            if (m_keyboard.Acquire().IsFailure)
                return null;

            if (m_keyboard.Poll().IsFailure)
                return null;

            KeyboardState keyboardState = m_keyboard.GetCurrentState();

            if (Result.Last.IsFailure)
                return null;

            return keyboardState;
        }

        public MouseState ReadMouse()
        {
            if (m_mouse.Acquire().IsFailure)
                return null;

            if (m_mouse.Poll().IsFailure)
                return null;

            MouseState mouseState = m_mouse.GetCurrentState();

            if (Result.Last.IsFailure)
                return null;

            return mouseState;
        }

        public virtual void Dispose()
        {
            if (m_keyboard != null)
            {
                m_keyboard.Unacquire();
                m_keyboard.Dispose();
            }
            m_keyboard = null;

            if (m_mouse != null)
            {
                m_mouse.Unacquire();
                m_mouse.Dispose();
            }
            m_mouse = null;

            m_dinput.Dispose();
        }

    }
}
