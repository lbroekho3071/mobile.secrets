using System.Text;

namespace Mobile.Secrets.Utils
{
    public class IndentationStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
    
        private int _indentation;
    
        public IndentationStringBuilder(int indentation)
        {
            _indentation = indentation;
        
            _stringBuilder = new StringBuilder();
        }

        public IndentationStringBuilder AppendLine(string value)
        {
            _stringBuilder
                .Append(new string('\t', _indentation))
                .AppendLine(value);

            return this;
        }

        public IndentationStringBuilder IncrementIndentation()
        {
            _indentation++;
            return this;
        }

        public IndentationStringBuilder DecrementIndentation()
        {
            _indentation--;
            return this;
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
