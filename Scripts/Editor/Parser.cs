using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace CultistLike
{
    public class EntityParser
    {
        public delegate void EntityCallback(
            char entityType,
            string name,
            List<string> unnamedProperties,
            Dictionary<char, string> namedProperties);

        private readonly EntityCallback _onEntity;

        public EntityParser(EntityCallback onEntity)
        {
            _onEntity = onEntity
                ?? throw new ArgumentNullException(nameof(onEntity));
        }

        public void ParseFile(string path)
        {
            using var reader = File.OpenText(path);
            Parse(reader);
        }

        public void Parse(TextReader reader)
        {
            char? entityType = null;
            string label = "";

            var unnamedProperties = new List<string>();
            var namedProperties = new Dictionary<char, string>();

            var currentKey = '\0'; // '\0' means the entity label
            var content = new StringBuilder();

            bool IsPipeLine(string line) => (line.Length > 0 && line[0] == '|') || (line.Length > 1 && line[1] == '|');
            bool IsEnd(string line) =>  (line.Length > 0 && line[0] == '|') && (line.Length > 1 && line[1] == '|');
            bool IsEntity(string line) => Char.IsUpper(line, 0) && line[1] == '|';
            bool IsNProperty(string line) => Char.IsLower(line, 0) && line[1] == '|';
            bool IsUProperty(string line) => line[0] == '|';

            void FinishField()
            {
                if (currentKey == '\0')
                {
                    return;
                }
                if (currentKey == '!')
                {
                    content.Clear();
                    return;
                }

                string value = content.ToString();
                content.Clear();

                if (currentKey == ' ')
                {
                    unnamedProperties.Add(value);
                }
                else
                {
                    namedProperties[currentKey] = value;
                }
            }

            void FinishEntity()
            {
                if (entityType is null)
                    return;

                _onEntity(
                    entityType.Value,
                    label,
                    unnamedProperties,
                    namedProperties);

                entityType = null;
                label = "";
                unnamedProperties = new List<string>();
                namedProperties = new Dictionary<char, string>();
                currentKey = '\0';
            }

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length <=1 && IsPipeLine(line) == false)
                {
                    if (currentKey != '\0')
                    {
                        content.Append('\n');
                        content.Append(line);
                    }
                    continue;

                }

                if (IsPipeLine(line))
                {
                    TrimTrailingWhitespace(content);
                    if (IsEnd(line))
                    {
                        FinishField();
                        FinishEntity();
                        currentKey = '!';
                    }
                    else if (IsUProperty(line))
                    {
                        FinishField();
                        currentKey = ' ';
                        content.Append(line.Substring(1));
                    }
                    else if (IsNProperty(line))
                    {
                        FinishField();
                        currentKey = line[0];
                        content.Append(line.Substring(2));
                    }
                    else if (IsEntity(line))
                    {
                        FinishField();
                        FinishEntity();
                        currentKey = '\0';
                        entityType = line[0];
                        label = line.Substring(2);
                    }
                }
                else
                {
                    content.Append('\n');
                    content.Append(line);
                }
            }

            FinishField();
            FinishEntity();
        }

        private static void TrimTrailingWhitespace(StringBuilder value)
        {
            while (value.Length > 0 &&
                char.IsWhiteSpace(value[value.Length - 1]))
            {
                value.Length--;
            }
        }
    }
}