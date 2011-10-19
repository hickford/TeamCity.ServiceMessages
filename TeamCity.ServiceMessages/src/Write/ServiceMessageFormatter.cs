/*
 * Copyright 2007-2011 JetBrains s.r.o.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using System.Linq;

namespace JetBrains.TeamCity.ServiceMessages.Write
{
  public class ServiceMessageFormatter
  {
    private const string SERVICE_MESSAGE_HEADER = "##teamcity[";

    public static bool ContainsServiceMessage(string text)
    {
      return text.Contains(SERVICE_MESSAGE_HEADER);
    }

    /// <summary>
    /// Performs TeamCity-format escaping of a string.
    /// </summary>
    /// <returns></returns>
    private static string Escape([NotNull] string value)
    {
      return value
        .Replace("|", "||")
        .Replace("\'", "|'")
        .Replace("\n", "|n")
        .Replace("\r", "|r")
        .Replace("]", "|]")
        .Replace("\u0085", "|x") // \u0085 (next line)	 |x
        .Replace("\u2028", "|l") //  \u2028 (line separator)	 |l
        .Replace("\u2029", "|p") //  \u2028 (line separator)	 |l
        ;     
    }

    public static string FormatMessage([NotNull] string messageName, [NotNull] string singleValue)
    {
      if (string.IsNullOrEmpty(messageName))
        throw new ArgumentNullException("messageName");
      if (singleValue == null)
        throw new ArgumentNullException("singleValue");

      if (Escape(messageName) != messageName)
        throw new ArgumentException("The message name contains illegal characters.", "messageName");

      return string.Format("{2}{0} '{1}']", messageName, Escape(singleValue), SERVICE_MESSAGE_HEADER);
    }

    public static string FormatMessage([NotNull] string messageName, [NotNull] object anonymousProperties)
    {
      if (string.IsNullOrEmpty(messageName))
        throw new ArgumentNullException("messageName");
      if (anonymousProperties == null)
        throw new ArgumentNullException("anonymousProperties");

      var properties = anonymousProperties.GetType().GetProperties();
      return FormatMessage(
        messageName,
        properties.Select(x => new ServiceMessageProperty(x.Name, x.GetValue(anonymousProperties, null).ToString())));
    }

    public static string FormatMessage([NotNull] string messageName, [NotNull] params ServiceMessageProperty[] properties)
    {
      return FormatMessage(messageName, properties.ToList());
    }

    public static string FormatMessage([NotNull] string messageName, [NotNull] IEnumerable<ServiceMessageProperty> properties)
    {
      if (string.IsNullOrEmpty(messageName))
        throw new ArgumentNullException("messageName");
      if (properties == null)
        throw new ArgumentNullException("properties");

      if (Escape(messageName) != messageName)
        throw new ArgumentException("The message name contains illegal characters.", "messageName");

      if (Escape(messageName) != messageName)
        throw new ArgumentException("Message name contains illegal characters", "messageName");

      var sb = new StringBuilder();
      sb.Append(SERVICE_MESSAGE_HEADER);
      sb.Append(messageName);

      foreach (ServiceMessageProperty property in properties)
      {
        if (Escape(property.Key) != property.Key)
          throw new InvalidOperationException(string.Format("The property name �{0}� contains illegal characters.", property.Key));
        sb.AppendFormat(" {0}='{1}'", property.Key, Escape(property.Value));
      }
      sb.Append(']');

      return sb.ToString();
    }
  }
}