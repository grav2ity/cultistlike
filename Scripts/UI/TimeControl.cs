using UnityEngine;
using UnityEngine.UI;


namespace CultistLike
{
    public class TimeControl : MonoBehaviour
    {
        public Color selectedColor;
        public Color defaultColor;

        [SerializeField] private Button selectedButton;
        private Button[] buttons;

        public void SelectButton(Button selectedButton)
        {
            foreach (var button in buttons)
            {
                button.GetComponent<Image>().color = defaultColor;
            }

            this.selectedButton = selectedButton;
            selectedButton.GetComponent<Image>().color = selectedColor;
        }

        private void Awake()
        {
            buttons = GetComponentsInChildren<Button>();
            if (selectedButton != null)
            {
                SelectButton(selectedButton);
            }
        }
    }
}
