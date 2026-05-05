namespace FireAlt.BLinq.Generators.Templates
{
    using System;
    using System.Text;

    internal abstract class TextTemplate
    {
        private readonly StringBuilder builder = new StringBuilder();
        private int indent;

        public string TransformText()
        {
            builder.Clear();
            indent = 0;

            Render();

            return builder.ToString();
        }

        protected abstract void Render();

        protected void WriteLine()
        {
            builder.AppendLine();
        }

        protected void WriteLine(string value)
        {
            builder.Append(' ', indent * 4);
            builder.AppendLine(value);
        }

        protected void PushIndent()
        {
            indent++;
        }

        protected void PopIndent()
        {
            indent--;
        }

        protected void Block(string declaration, Action body)
        {
            WriteLine(declaration);
            Block(body);
        }

        protected void Block(Action body)
        {
            WriteLine("{");
            PushIndent();
            body();
            PopIndent();
            WriteLine("}");
        }

        protected void AggressiveInlining()
        {
            WriteLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        }
    }
}
