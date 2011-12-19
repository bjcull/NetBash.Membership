using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using NDesk.Options;
using System.IO;

namespace NetBash.Membership.Commands
{
    [WebCommand("role", "Manage membership roles using the default provider")]
    public class RoleCommand : IWebCommand
    {
        private Command? _command;
        private RoleProvider _provider;

        public RoleCommand()
        {
            _provider = System.Web.Security.Roles.Provider;
        }

        public string Process(string[] args)
        {
            var sb = new StringBuilder();

            var p = new OptionSet() {
                { "c|create", "create a role\nUSAGE: role --create rolename",
                    v => _command = Command.Create },
                { "d|delete", "delete a role\nUSAGE: role --delete rolename",
                    v => _command = Command.Delete },
                { "l|list", "return a list of roles\nUSAGE: role --list",
                    v => _command = Command.List },
                { "g|give", "add a user to a role\nUSAGE: role --give username rolename",
                    v => _command = Command.Give },
                { "t|take", "remove a user from a role\nUSAGE: role --take username rolename",
                    v => _command = Command.Take },
                { "u|users", "list all users in a role\nUSAGE: role --users rolename",
                    v => _command = Command.Users },
                { "r|roles", "list all roles for a user\nUSAGE: role --roles username",
                    v => _command = Command.Roles },
                { "h|help", "show this list of options\nUSAGE --help",
                    v => _command = null }
            };

            List<string> extras;
            try
            {
                extras = p.Parse(args);
            }
            catch (OptionException e)
            {
                sb.Append("role: ");
                sb.AppendLine(e.Message);
                sb.AppendLine("Try `role --help' for more information.");
                return sb.ToString();
            }

            // perform the selected command
            if (_command == Command.Create)
            {
                if (extras.Count == 1)
                {
                    _provider.CreateRole(extras[0]);
                    sb.AppendLine("Role successfully created");
                }
                else
                {
                    sb.AppendLine("USAGE: role --create rolename");
                }
            }
            else if (_command == Command.Delete)
            {
                if (extras.Count == 1)
                {
                    try
                    {
                        var success = _provider.DeleteRole(extras[0], true);

                        if (success)
                        {
                            sb.AppendLine("Role successfully deleted");
                        }
                        else
                        {
                            sb.AppendFormat("Could not delete role: {0}", extras[0]);
                            sb.AppendLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendFormat("Could not delete role: {0} - {1}", extras[0], ex.Message);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: role --delete rolename");
                }
            }
            else if (_command == Command.List)
            {
                if (extras.Count == 0)
                {
                    var roles = _provider.GetAllRoles();

                    if (roles.Count() > 0)
                    {
                        foreach (var role in roles)
                        {
                            sb.AppendLine(role);
                        }
                    }
                    else
                    {
                        sb.AppendLine("No roles found");
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: role --list");
                }
            }
            else if (_command == Command.Give)
            {
                if (extras.Count == 2)
                {
                    _provider.AddUsersToRoles(new string[] { extras[0] }, new string[] { extras[1] });
                    sb.AppendFormat("Added {0} to {1}", extras[0], extras[1]);
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("USAGE: role --give username rolename");
                }
            }
            else if (_command == Command.Take)
            {
                if (extras.Count == 2)
                {
                    _provider.RemoveUsersFromRoles(new string[] { extras[0] }, new string[] { extras[1] });
                    sb.AppendFormat("Removed {0} from {1}", extras[0], extras[1]);
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("USAGE: role --take username rolename");
                }
            }
            else if (_command == Command.Users)
            {
                if (extras.Count == 1)
                {
                    var users = _provider.GetUsersInRole(extras[0]);

                    if (users.Count() > 0)
                    {
                        sb.AppendFormat("{0} users in {1}", users.Count(), extras[0]);
                        sb.AppendLine();

                        foreach (var u in users)
                        {
                            sb.AppendLine(u);
                        }
                    }
                    else
                    {
                        sb.AppendLine("No users in role");
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: role --users rolename");
                }
            }
            else if (_command == Command.Roles)
            {
                if (extras.Count == 1)
                {
                    var roles = _provider.GetRolesForUser(extras[0]);

                    if (roles.Count() > 0)
                    {
                        sb.AppendFormat("{0} roles for {1}", roles.Count(), extras[0]);
                        sb.AppendLine();

                        foreach (var r in roles)
                        {
                            sb.AppendLine(r);
                        }
                    }
                    else
                    {
                        sb.AppendLine("No roles found for user");
                    }
                }
                else
                {
                    sb.AppendLine("USAGE: role --roles username");
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

            sb.WriteLine("Usage: role [OPTIONS]");
            sb.WriteLine("Manage membership roles using the default provider");
            sb.WriteLine();
            sb.WriteLine("Options:");

            p.WriteOptionDescriptions(sb);

            return sb.ToString();
        }

        public bool ReturnHtml
        {
            get { return false; }
        }

        private enum Command
        {
            Create,
            Delete,
            List,
            Give,
            Take,
            Users,
            Roles
        }
    }
}
