using UnityEngine;
using UnityEngine.UIElements;

namespace WaxCloudWalletUnity.Examples.Ui
{
    [RequireComponent(typeof(UIDocument))]
    public class ScreenBase : MonoBehaviour
    {
        internal VisualElement Root;

        private UIDocument _screen;

        private void Awake()
        {
            _screen = GetComponent<UIDocument>();
            Root = _screen.rootVisualElement;

            Hide();
        }

        public void Show()
        {
            Root.Show();
        }

        public void Hide()
        {
            Root.Hide();
        }
    }

    public static class Utils
    {
        /// <summary>
        /// Extension-method to show an UI Element (set it to visible)
        /// </summary>
        /// <param name="element"></param>
        public static void Show(this VisualElement element)
        {
            if (element == null)
                return;

            element.style.visibility = Visibility.Visible;
            element.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Extension-method to hide an UI Element (set it to invisible)
        /// </summary>
        /// <param name="element"></param>
        public static void Hide(this VisualElement element)
        {
            if (element == null)
                return;

            element.style.visibility = Visibility.Hidden;
            element.style.display = DisplayStyle.None;
        }
    }
}