/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Conf;
using Azos.Collections;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Decorates MVC Actions
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public class ActionAttribute : Attribute
  {
    public ActionAttribute()
    {

    }

    public ActionAttribute(int order)
    {
      m_Order =order;
    }


    /// <summary>
    /// Specifies name override
    /// </summary>
    public ActionAttribute(string name, int order, bool strictParamBinding = false)
    {
      m_Name = name;
      m_Order = order;
      m_StrictParamBinding = strictParamBinding;
    }

    /// <summary>
    /// Specifies name override and WorkMatch instances in Laconic format. i.e.:
    ///  'match{name="A" order=0 is-local=false} match{name="B" order=0 methods="post,put" is-local=true}'
    /// </summary>
    public ActionAttribute(string name, int order, string matchScript,  bool strictParamBinding = false)
    {
      m_Name = name;
      m_Order = order;
      m_MatchScript = matchScript;
      m_StrictParamBinding = strictParamBinding;
    }

    private string m_Name;
    private int m_Order;
    private string m_MatchScript;
    private OrderedRegistry<WorkMatch> m_Matches;
    private bool m_StrictParamBinding;


    /// <summary>
    /// When set, specifies the invocation name override, null by default which means that the name of decorated member should be used
    /// </summary>
    public string Name{get{return m_Name;}}

    /// <summary>
    /// Dictates the match making order of actions within the group
    /// </summary>
    public int Order{get{return m_Order;}}

    /// <summary>
    /// Returns match script in Laconic config format if the one was supplied or empty string
    /// </summary>
    public string MatchScript {get{return m_MatchScript ?? string.Empty;}}


    /// <summary>
    /// Returns true if default parameter binder should not perform indirect value conversions, i.e. integer tick number as date time.
    /// False by default
    /// </summary>
    public bool StrictParamBinding { get{ return m_StrictParamBinding;}}

    /// <summary>
    /// Returns ordered matches configured from script or empty enum
    /// </summary>
    public IEnumerable<WorkMatch> Matches
    {
      get
      {
        if (m_Matches==null)
        {
          m_Matches = new OrderedRegistry<WorkMatch>();
          if (m_MatchScript.IsNotNullOrWhiteSpace())
          {
            ConfigSectionNode root;
            try
            {
              root = LaconicConfiguration.CreateFromString("root{{{0}}}".Args(m_MatchScript)).Root;
            }
            catch(Exception error)
            {
              throw new WaveException(StringConsts.MVC_ACTION_ATTR_MATCH_PARSING_ERROR.Args(m_MatchScript, error.ToMessageWithType()), error);
            }

            foreach(var cn in root.Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
              if(!m_Matches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
               throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "ActionAttribute"));
          }
          else
           m_Matches.Register(new WorkMatch("*",0));
        }
        return m_Matches.OrderedValues;
      }
    }

  }


  /// <summary>
  /// General ancestor for MVC Action Filters - get invoked before and after actions
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public abstract class ActionFilterAttribute : Attribute
  {
    protected ActionFilterAttribute()
    {

    }

    protected ActionFilterAttribute(int order)
    {
      Order =order;
    }

    /// <summary>
    /// Dictates the call order
    /// </summary>
    public readonly int Order;

    /// <summary>
    /// Override to add logic/filtering right before the invocation of action method.
    /// Return TRUE to indicate that request has already been handled and no need to call method body and AfterActionInvocation in which case
    ///  return result via 'out result' paremeter
    /// </summary>
    protected internal abstract bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result);

    /// <summary>
    /// Override to add logic/filtering right after the invocation of action method. Must return TRUE to stop processing chain
    /// </summary>
    protected internal abstract bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result);

    /// <summary>
    /// Override to add logic/filtering finally after the invocation of action method. Must return TRUE to stop processing chain
    /// </summary>
    protected internal abstract void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result);
  }




}
