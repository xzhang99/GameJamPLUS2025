////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace FronkonGames.LUTs.Horror
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Enums. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Horror
  {
    /// <summary> Modes. </summary>
    public enum Modes
    {
      /// <summary> Maximum quality. Recommended for PC and consoles. </summary>
      Quality,

      /// <summary> Improved performance. Recommended for mobile devices. </summary>
      Performance,
    }

    /// <summary> Tonemap operators. </summary>
    public enum Tonemappers
    {
      None,
      Neutral,
      ACES,
      Reinhard,
    }
  }
}
