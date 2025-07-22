using UnityEngine;
using UnityEngine.UI;


namespace ParkourFPS
{
    public class CursorState : MonoBehaviour
    {
        [Tooltip("if mouse is locked to the middle of the screen")]
        [SerializeField] private bool mouseLocked = false;
        [Tooltip("if the cursor is visible")]
        [SerializeField] private bool cursorVisible = false;

        private void Start()
        {
            SetCursor(mouseLocked, cursorVisible);
        }

        public void SetCursor(bool mouseLocked, bool cursorVisible)
        {
            Cursor.lockState = mouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = cursorVisible;
        }
    }
}