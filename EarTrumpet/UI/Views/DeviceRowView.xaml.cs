using System.Windows.Documents;

namespace EarTrumpet.UI.Views
{
    public partial class DeviceRowView
    {
        public DeviceRowView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Focus the row without leaving a focus rectangle behind, which is
        /// what the flyout wants when it is opened by mouse. Mirrors
        /// DeviceView, including reaching the element from inside this
        /// control's own XAML rather than by name from the window: a type
        /// defined in this assembly has no generated field in the window's
        /// temporary-assembly pass.
        /// </summary>
        public void FocusAndRemoveFocusVisual()
        {
            DeviceNameButton.Focus();

            var adornerLayer = AdornerLayer.GetAdornerLayer(DeviceNameButton);
            var adorners = adornerLayer?.GetAdorners(DeviceNameButton);
            if (adorners != null)
            {
                foreach (var adorner in adorners)
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }
    }
}
