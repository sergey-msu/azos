/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Azos.CodeAnalysis.Source
{
    /// <summary>
    /// Represents source code stored in a stream
    /// </summary>
    public class StreamSource : StreamReader, ISourceText
    {
      /// <summary>
      /// Constructs stream source with specified language and default encoding
      /// </summary>
      public StreamSource(Stream stream, Language language, string name = null)
        : base(stream)
      {
        m_Language = language;
        m_Name = name;
      }

      /// <summary>
      /// Constructs stream source with specified language and encoding
      /// </summary>
      public StreamSource(Stream stream, Encoding encoding, Language language, string name = null)
        : base(stream, encoding)
      {
        m_Language = language;
        m_Name = name;
      }


      private Language m_Language;
      private string m_Name;


      public void Reset()
      {
        BaseStream.Position = 0;
        DiscardBufferedData();
      }


      /// <summary>
      /// Returns source's name
      /// </summary>
      public string Name
      {
        get { return m_Name ?? string.Empty; }
      }

      public bool EOF
      {
        get
        {
          return EndOfStream;
        }
      }

      public char ReadChar()
      {

        return (char)Read();
      }

      public char PeekChar()
      {
        return (char)Peek();
      }

      public Language Language
      {
        get { return m_Language ?? UnspecifiedLanguage.Instance; }
      }
    }
}
