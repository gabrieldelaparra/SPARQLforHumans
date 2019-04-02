using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleKit
{
    public class Menu
    {
        /// <summary>
        ///     The selected color to highlight the option with.
        /// </summary>
        public ConsoleColor HighlightColor;

        /// <summary>
        ///     A list of options to choose. Indexing and numeric quantifying is handled automatically.
        /// </summary>
        public string[] Options;

        /// <summary>Prompts for input, handles everything. Returns the corresponding choice against the Options.</summary>
        /// <param name="selected"> Which item/position should be highlighted on print. Ignore for first.</param>
        /// <code>int choice = menu.AwaitInput();</code>
        public int AwaitInput(int selected = 0, string title = "")
        {
            if (!string.IsNullOrWhiteSpace(title))
                Console.WriteLine(title);

            for (var i = 0; i < Options.Length; i++)
            {
                if (selected == i)
                    Console.ForegroundColor = HighlightColor;

                Console.WriteLine("{0}.\t{1}", i + 1, Options[i]);
                Console.ResetColor();
            }

            var input = Console.ReadKey();
            var info = TranslateKeyInput(input.Key, selected);

            Console.Clear();

            if (info == -1)
                return selected;
            return AwaitInput(info);
        }

        private int TranslateKeyInput(ConsoleKey key, int selected)
        {
            var max = Options.Length - 1;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    return selected == 0 ? max : selected - 1;
                case ConsoleKey.DownArrow:
                    return selected == max ? 0 : selected + 1;
                case ConsoleKey.Enter:
                    return -1;
                default:
                    return selected;
            }
        }
    }

    public class Table
    {
        private readonly int _indent;
        private readonly int _tableWidth;

        /// <summary>Initializes the Table.</summary>
        /// <param name="width"> Width of the table.</param>
        /// <param name="indent"> A spacing between the console window border and the start of the grid - purely aesthetic.</param>
        public Table(int width, int indent)
        {
            _tableWidth = width;
            _indent = indent;
        }

        /// <summary>Builds a table from a list of custom types. Automatically pulls property names and all corresponding values.</summary>
        /// <param name="items"> A collection of items to populate with. Must hold at least 1 item.</param>
        public void BuildTable<T>(List<T> items)
        {
            for (var i = 0; i < _indent; i++)
                Console.WriteLine();

            var props = typeof(T).GetProperties();
            var names = props.Select(p => p.Name);

            PrintRow(names);

            for (var i = 0; i < items.Count; i++)
                PrintRow(props.Select(p => p.GetValue(items[i])?.ToString()));
        }

        /// <summary>Prints a header-less row for easy formatting.</summary>
        /// <param name="items"> A collection of items to print. Must hold at least 1 item.</param>
        /// <param name="divider"> Whether or not to include a divider below the entry.</param>
        public void PrintRow<T>(IEnumerable<T> items, bool divider = true)
        {
            PrintIndent();

            var length = items.Count();
            var width = (_tableWidth - length) / length;

            foreach (var column in items)
                Console.Write(BuildCellString(column?.ToString(), width) + '|');

            Console.WriteLine();

            if (divider)
                PrintDivider();
        }

        private void PrintIndent()
        {
            Console.Write(new string(' ', _indent * 2));
        }

        private void PrintDivider()
        {
            PrintIndent();
            Console.WriteLine(new string('-', _tableWidth - 1));
        }

        private string BuildCellString(string text, int width)
        {
            if (text?.Length > width)
                text = Ellipsis(text, width);

            if (string.IsNullOrEmpty(text))
                return new string(' ', width);
            return text?.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }

        private string Ellipsis(string text, int width)
        {
            return text.Substring(0, width - 3) + new string('.', 3);
        }
    }

    public static class Validator
    {
        /// <summary>Prompts and attempts to convert input from the user.</summary>
        /// <param name="prompt"> Instructions to prompt the user. ": " is appended automatically for cleaner formatting.</param>
        /// ///
        /// <param name="retry"> A message if conversion has failed i.e. user must retry.</param>
        public static T GetInput<T>(string prompt, string retry)
        {
            Console.Write(prompt + ": ");
            var input = Console.ReadLine();

            try
            {
                return (T) Convert.ChangeType(input, typeof(T));
            }
            catch
            {
                Console.WriteLine(retry);
                return GetInput<T>(prompt, retry);
            }
        }

        /// <summary>Prompts and validates input against a lambda expression.</summary>
        /// <param name="prompt"> Instructions to prompt the user. ": " is appended automatically for cleaner formatting.</param>
        /// <param name="retry"> A message if conversion has failed i.e. user must retry.</param>
        /// <param name="comparer"> Lambda expression to compare input against. Predicate variable must match input type.</param>
        public static T ValidateInput<T>(string prompt, string retry, Func<T, bool> comparer)
        {
            T input;
            var valid = false;

            do
            {
                input = GetInput<T>(prompt, retry);
                valid = comparer(input);

                if (!valid)
                    Console.WriteLine(retry);
            } while (!valid);

            return (T) Convert.ChangeType(input, typeof(T));
        }
    }
}