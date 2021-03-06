/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Media.PDF.DocumentModel
{
  /// <summary>
  /// Represents entities that has string representation in PDF
  /// </summary>
  public interface IPdfWritable
  {
    /// <summary>
    /// Returns PDF string representation
    /// </summary>
    string ToPdfString();
  }
}