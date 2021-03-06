/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{
    public class Appl : Cmdlet
    {
        private const string VAL = "|<f color=yellow>{0}<f color=gray>\n" ;

        public const string CONFIG_STOP_NOW_HR_ATTR = "stop-now-hour";


        public Appl(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var stopNowHour = m_Args.AttrByName(CONFIG_STOP_NOW_HR_ATTR).ValueAsInt(-10);
            if (stopNowHour == App.LocalizedTime.Hour)
            {
              var text = StringConsts.APPL_CMD_STOPPING_INFO.Args(m_Terminal.Name, m_Terminal.WhenConnected, m_Terminal.Who);
              App.Log.Write( new Azos.Log.Message
              {
                 Type = Azos.Log.MessageType.Warning,
                 Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
                 From = "{0}.StopNow".Args(GetType().FullName),
                 Text = text
              });
              App.Stop();
              return text;//noone may see this as App may terminate faster than response delivered
            }




            var sb = new StringBuilder(1024);
            sb.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
            sb.AppendLine("<push><f color=gray>");
            sb.AppendLine("Application Container");
            sb.AppendLine("----------------------------------------------------------------------------");
            sb.AppendFormat("Name                      "+VAL, App.Name );
            sb.AppendFormat("Host                      "+VAL, App.HostName );
            sb.AppendFormat("Parent Zone Governor      "+VAL, App.ParentZoneGovernorPrimaryHostName ?? "[none, this host is top level]" );
            sb.AppendFormat("Role                      "+VAL, App.GetThisHostMetabaseSection().RoleName );
            sb.AppendFormat("Role Apps                 "+VAL, App.GetThisHostMetabaseSection().Role.AppNames.Aggregate("",(r,a)=>r+a+", ") );
            sb.AppendFormat("Metabase App              "+VAL, App.MetabaseApplicationName );
            sb.AppendFormat("Instance ID               "+VAL, App.InstanceID );
            sb.AppendFormat("Start Time                "+VAL, App.StartTime );
            sb.AppendFormat("Running Time              "+VAL, App.LocalizedTime - App.StartTime );
            sb.AppendFormat("Type                      "+VAL, App.GetType().FullName );
            sb.AppendFormat("Active                    "+VAL, App.Active);
            sb.AppendFormat("Boot Conf Root            "+VAL, App.BootConfigRoot );
            sb.AppendFormat("Conf Root                 "+VAL, App.ConfigRoot );
            sb.AppendFormat("Data Store                "+VAL, App.DataStore.GetType().FullName );
            sb.AppendFormat("Glue                      "+VAL, App.Glue.GetType().FullName );
            sb.AppendFormat("Instrumentation.          "+VAL, App.Instrumentation.GetType().FullName );
            sb.AppendFormat("Localized Time            "+VAL, App.LocalizedTime );
            sb.AppendFormat("Time Location             "+VAL, App.TimeLocation );
            sb.AppendFormat("Log                       "+VAL, App.Log.GetType().FullName );
            sb.AppendFormat("Object Store              "+VAL, App.ObjectStore.GetType().FullName );
            sb.AppendFormat("Security Manager          "+VAL, App.SecurityManager.GetType().FullName );
            sb.AppendFormat("Module Root               "+VAL, App.ModuleRoot.GetType().FullName );
            sb.AppendFormat("TimeSource                "+VAL, App.TimeSource.GetType().FullName );

            var lwarning = App.Log.LastWarning;
            sb.AppendFormat("Last Warning              "+VAL, lwarning!=null ? "{0} {1} {2} {3}".Args(lwarning.UTCTimeStamp, lwarning.Guid, lwarning.From, lwarning.Text) : string.Empty );

            var lerror = App.Log.LastError;
            sb.AppendFormat("Last Error                "+VAL, lerror!=null ? "{0} {1} {2} {3}".Args(lerror.UTCTimeStamp, lerror.Guid, lerror.From, lerror.Text) : string.Empty );

            var lcatastrophe = App.Log.LastCatastrophe;
            sb.AppendFormat("Last Catastrophe          "+VAL, lcatastrophe!=null ? "{0} {1} {2} {3}".Args(lcatastrophe.UTCTimeStamp, lcatastrophe.Guid, lcatastrophe.From, lcatastrophe.Text) : string.Empty );


            sb.AppendLine();
            sb.AppendLine("Root Components");
            sb.AppendLine("----------------------------------------------------------------------------");
            var all = App.AllComponents;
            foreach(var cmp in all.Where(cmp => !(cmp.ComponentDirector is IApplicationComponent)))
            {
              string name = null;
              var named = cmp as INamed;
              if (named!=null) name = named.Name;

              sb.AppendLine("<f color=white>SID: <f color=yellow>{0,-4:D4}<f color=gray> {1} {2} <f color=magenta>{3} <f color=green>{4}".Args(cmp.ComponentSID,
                                                                                                                                              DetailedComponentDateTime(cmp.ComponentStartTime),
                                                                                                                                              cmp.GetType().FullName,
                                                                                                                                              cmp.ComponentCommonName, name));

              var children = all.Where(c => object.ReferenceEquals(c.ComponentDirector, cmp));
              foreach(var child in children)
              {
                string cname = null;
                var cnamed = child as INamed;
                if (cnamed!=null) cname = cnamed.Name;
                sb.AppendLine("<f color=darkgray> -> <f color=cyan>{0,-6:D4}<f color=darkgray> {1} {2} <f color=magenta>{3} <f color=darkgreen>{4}".Args(child.ComponentSID,
                                                                                                                                              DetailedComponentDateTime(child.ComponentStartTime),
                                                                                                                                              child.GetType().Name,
                                                                                                                                              child.ComponentCommonName,
                                                                                                                                              cname));
              }

              sb.AppendLine();
            }

            sb.AppendLine("<f color=darkgray>  * Note: this command only outputs root components and 1 level of their immediate children");
            sb.AppendLine("<pop>");

            return sb.ToString();
        }


        public override string GetHelp()
        {
            return
@"Displays the status of the Application Container:
           Pass <f color=yellow>stop-now-hour<f color=gray>=app_local_hour to stop the App.
           This is needed so that an inadvertent stopping of the App container
           is precluded. The parameter must match the current local App time
";
        }

        public static string DetailedComponentDateTime(DateTime date)
        {
          return "[{0:D2}/{1:D2} {2:D2}:{3:D2}:{4:D2}.{5:D3}]".Args(date.Month,
                                                      date.Day,
                                                      date.Hour,
                                                      date.Minute,
                                                      date.Second,
                                                      date.Millisecond);
        }


    }

}
