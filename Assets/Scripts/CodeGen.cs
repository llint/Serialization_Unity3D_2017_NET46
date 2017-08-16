using System;
using System.Text;
using System.Collections.Generic;

namespace CodeGen
{
    abstract class CodeElement
    {
        internal CodeElement parent = null;

        internal abstract string Content { get; }

        protected int IndentLevel
        {
            get
            {
                if (parent == null)
                {
                    return 0;
                }
                return parent.IndentLevel + 1;
            }
        }
    }

    class CodeLine : CodeElement
    {
        string line;

        internal CodeLine(string line_ = "")
        {
            line = line_;
        }

        internal override string Content
        {
            get
            {
                string indent = new String(' ', IndentLevel * 4);
                var r = new StringBuilder();

                return r.AppendLine(indent + line).ToString();
            }
        }
    }

    class CodeBlock : CodeElement
    {
        string suffix = "";

        internal CodeBlock WithComma()
        {
            suffix = ",";
            return this;
        }

        internal CodeBlock WithSemicolon()
        {
            suffix = ";";
            return this;
        }

        // returns the current containing block
        internal CodeBlock AddLine(string s = "")
        {
            var line = new CodeLine(s);
            line.parent = this;
            children.Add(line);
            return this;
        }

        // returns the new block
        // NB: the block
        internal CodeBlock AddBlock()
        {
            var block = new CodeBlock();
            block.parent = this;
            children.Add(block);
            return block;
        }

        internal CodeGroup AddGroup()
        {
            var group = new CodeGroup();
            group.parent = this;
            children.Add(group);
            return group;
        }

        List<CodeElement> children = new List<CodeElement>();

        internal override string Content
        {
            get
            {
                var r = new StringBuilder();
                string indent = new String(' ', IndentLevel * 4);
                r.AppendLine(indent + "{");
                foreach (var element in children)
                {
                    r.Append(element.Content);
                }
                r.AppendLine(indent + "}" + suffix);
                return r.ToString();
            }
        }
    }

    internal class CodeGroup : CodeElement
    {
        List<CodeElement> children = new List<CodeElement>();

        internal CodeGroup AddLine(string s = "")
        {
            var line = new CodeLine(s);
            line.parent = parent; // NB: grouping doesn't add nesting level
            children.Add(line);
            return this;
        }

        internal CodeBlock AddBlock()
        {
            var block = new CodeBlock();
            block.parent = parent; // NB: grouping doesn't add nesting level
            children.Add(block);
            return block;
        }

        internal CodeGroup AddGroup()
        {
            var group = new CodeGroup();
            group.parent = parent; // NB: grouping doesn't add nesting level
            children.Add(group);
            return group;
        }

        internal override string Content
        {
            get
            {
                var r = new StringBuilder();
                foreach (var element in children)
                {
                    r.Append(element.Content);
                }
                return r.ToString();
            }
        }
    }
}
