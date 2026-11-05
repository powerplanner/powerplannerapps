using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    internal class SqliteWhereCommand
    {
        private readonly List<string> _commands = new();

        public void Add(string command)
        {
            _commands.Add(command);
        }

        /// <summary>
        /// If no commands, returns empty string. Otherwise, returns " WHERE XYZ " with spacing.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!_commands.Any())
            {
                return string.Empty;
            }

            var builder = new StringBuilder(" WHERE ");
            for (int i = 0; i < _commands.Count; i++)
            {
                builder.Append(_commands[i]);

                if (i < _commands.Count - 1)
                {
                    builder.Append(" and ");
                }
            }

            builder.Append(" ");

            return builder.ToString();
        }
    }
}
