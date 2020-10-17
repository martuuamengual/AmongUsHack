
using System.Windows.Forms;

namespace DirectX_Renderer.Interfaces
{
    /// <summary>
    ///  This interface includes
    ///  <list type="dotted">
    ///     <para>OnDrag()</para>
    ///  </list>
    /// </summary>
    public interface IDragHandler
    {
        /// <summary>
        ///  This event fires when mouse is left-click hold and mouse moves throw screen
        /// </summary>
        void OnDrag(object sender, Keys e);
    }
}
