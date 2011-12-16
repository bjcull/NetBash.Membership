using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using NetBash.Membership.Scripts;
using System.Web.Security;
using NetBash.Membership.Helpers;

namespace NetBash.Membership.Commands
{
    [WebCommand("user", "Manage membership users using the default provider")]
    public class UserCommand : IWebCommand
    {
        private Switch? _command;
        private MembershipProvider _provider;

        public UserCommand()
        {
            _provider = System.Web.Security.Membership.Provider;
        }

        public UserCommand(string defaultAnswer)
        {
            _provider = System.Web.Security.Membership.Provider;            
        }

        public string Process(string[] args)
        {
            var sb = new StringBuilder();

            var p = new OptionSet() {
                { "c|create", "create a user\nUSAGE: user --create username password email",
                    v => _command = Switch.Create },
                { "d|delete", "delete a user\nUSAGE: user --delete username",
                    v => _command = Switch.Delete },
                { "l|list", "return a list of users",
                    v => _command = Switch.List },
                { "r|reset", "reset a password\nUSAGE: user --reset username [answer]",
                    v => _command = Switch.Reset },
                { "f|find=", "find a user by username or email\nUSAGE: user --find=[name|email] query",
                    v => { if (v == "email") _command = Switch.FindEmail; else _command = Switch.FindName; } },
                { "u|unlock", "unlock a user\nUSAGE: user --unlock username",
                    v => _command = Switch.Unlock },
                { "o|online", "get number of online users\n USAGE:user --online",
                    v => _command = Switch.Online },
                { "i|install", "applies the sql membership schema to the given database",
                  v => _command = Switch.Install },
                { "h|help", "show this list of options",
                    v => _command = null }
            };

            List<string> extras;
            try
            {
                extras = p.Parse(args);
            }
            catch (OptionException e)
            {
                sb.Append("user: ");
                sb.AppendLine(e.Message);
                sb.AppendLine("Try `user --help' for more information.");
                return sb.ToString();
            }

            // perform the selected command
            if (_command == Switch.Create)
            {
                if (extras.Count == 3)
                {
                    MembershipCreateStatus status;
                    _provider.CreateUser(extras[0], extras[1], extras[2], "question", "anthony_rulz_da_skool", true, Guid.NewGuid(), out status);

                    if (status == MembershipCreateStatus.Success)
                    {
                        sb.AppendFormat("User successfully created: {0}", extras[0]);
                        sb.AppendLine();
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("Error: {0}", status.ToString()));
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --create username password email");
                }
            }
            else if (_command == Switch.Delete)
            {
                if (extras.Count == 1)
                {
                    var success = _provider.DeleteUser(extras[0], true);

                    if (success)
                    {
                        sb.AppendLine("User successfully deleted");
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("Could not delete user: {0}", extras[0]));
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --delete username");
                }
            }
            else if (_command == Switch.List)
            {
                var users = System.Web.Security.Membership.GetAllUsers();

                if (users.Count > 0)
                {
                    foreach (System.Web.Security.MembershipUser u in users)
                    {
                        sb.AppendFormat("{0} {1}", u.UserName.PadRight(20, ' '), u.Email);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("No users found");
                }
            }
            else if (_command == Switch.FindEmail)
            {
                if (extras.Count == 1)
                {
                    int totalRecords;
                    var users = _provider.FindUsersByEmail("%" + extras[0] + "%", 0, 100, out totalRecords);
                    
                    if (users.Count > 0)
                    {
                        sb.AppendFormat("{0} users found. query: {1}", totalRecords, extras[0]);
                        sb.AppendLine();

                        foreach (System.Web.Security.MembershipUser u in users)
                        {
                            sb.AppendFormat("{0} {1}", u.UserName.PadRight(20, ' '), u.Email);
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendFormat("No users found. query: {0}", extras[0]);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --find=email emailaddress");
                }
            }
            else if (_command == Switch.FindName)
            {
                if (extras.Count == 1)
                {
                    int totalRecords;
                    var users = _provider.FindUsersByName("%" + extras[0] + "%", 0, 100, out totalRecords);

                    if (users.Count > 0)
                    {
                        sb.AppendFormat("{0} users found. query: {1}", totalRecords, extras[0]);
                        sb.AppendLine();

                        foreach (System.Web.Security.MembershipUser u in users)
                        {
                            sb.AppendFormat("{0} {1}", u.UserName.PadRight(20, ' '), u.Email);
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendFormat("No users found. query: {0}", extras[0]);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --find=name");
                }
            }
            else if (_command == Switch.Online)
            {
                int online = _provider.GetNumberOfUsersOnline();
                if (online == 1)
                    sb.Append("There is currently 1 user online");
                else 
                    sb.AppendFormat("There are currently {0} users online", online);
                sb.AppendLine();
            }
            else if (_command == Switch.Reset)
            {
                if (extras.Count >= 1 && extras.Count <= 2)
                {
                    string answer = "";
                    if (extras.Count == 2)
                        answer = extras[1];

                    var result = _provider.ResetPassword(extras[0], answer);
                    if (!string.IsNullOrEmpty(result))
                    {
                        sb.AppendFormat("Password successfully reset.\nNew Password: {0}", result);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --reset username [answer]");
                }
            }
            else if (_command == Switch.Unlock)
            {
                if (extras.Count == 1)
                {
                    var success = _provider.UnlockUser(extras[0]);
                    if (success)
                    {
                        sb.AppendLine("User successfully unlocked");
                    }
                    else
                    {
                        sb.AppendFormat("Could not unlock user: {0}", extras[0]);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --unlock username");
                }
            }
            else if (_command == Switch.Install)
            {
                if (extras.Count == 1)
                {
                    try
                    {
                        sb.AppendLine(InstallMembershipSchema(extras[0]));
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format("Error: {0}", ex.Message));
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: user --install connectionStringName");
                }
            }
            else
            {
                sb.AppendLine(ShowHelp(p));
            }

            return sb.ToString();
        }

        private string ShowHelp(OptionSet p)
        {
            var sb = new StringWriter();

            sb.WriteLine("Usage: user [OPTIONS]");
            sb.WriteLine("Manage membership users using the default provider");
            sb.WriteLine();
            sb.WriteLine("Options:");

            p.WriteOptionDescriptions(sb);

            return sb.ToString();
        }

        private string InstallMembershipSchema(string connStringName)
        {
            //throw new NotImplementedException("This method is still under construction");
            var sb = new StringBuilder();

            var connString = ConfigurationManager.ConnectionStrings[connStringName].ConnectionString;
            sb.AppendFormat("Using connection string: {0}", connString);
            sb.AppendLine();

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("", conn))
                {
                    sb.AppendLine("Executing query...");

                    var helper = new SqlExecutionHelper();
                    helper.ExecuteBatchNonQuery(SQLScripts.MembershipSchema, cmd);
                    
                    sb.AppendLine("Success.");
                }

                conn.Close();
            }

            sb.AppendLine("SQL Membership Schema has been successfully applied.");
            sb.AppendLine();

            return sb.ToString();
        }

        public bool ReturnHtml
        {
            get { return false; }
        }

        private enum Switch
        {
            Create,
            Delete,
            List,
            Install,
            Password,
            Reset,
            FindEmail,
            FindName,
            Unlock,
            Online,
        }
    }
}
