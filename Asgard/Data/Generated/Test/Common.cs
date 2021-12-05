using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable IDE0066 // Convert switch statement to expression
#pragma warning disable IDE0021 // Use expression body for constructors

namespace Asgard.Generated.Test
{
    public class Loader
	{
		private readonly string filename;

		private readonly List<string> textLines = new();

		public List<EnumerationLine> EnumerationLines { get; } = new();
		public List<FileCommentLine> FileCommentLines { get; } = new();
		public List<HistoryLine> HistoryLines { get; } = new();
		public List<LicenceLine> LicenceLines { get; } = new();
		public List<OpCodeLine> OpCodeLines { get; } = new();
		public List<PropertyLine> PropertyLines { get; } = new();

		public List<byte> OpCodeNumbers { get; } = new();
		public List<string> EnumerationNames { get; } = new();

		public Loader(string filename)
		{
            this.filename = filename;
        }

		public void Load()
		{
			var textlines = File.ReadLines(this.filename);
			this.textLines.Clear();
			this.textLines.AddRange(textlines);

			var number = 1;
			var lines =
				this.textLines
					.Select(n => Line.Create(number, n))
					.Where(n => n != null)
					.ToDictionary(n => ++number, n => n);

			this.EnumerationLines.Clear();
			this.EnumerationLines.AddRange(
				lines
					.Select(n => n.Value as EnumerationLine)
                    .Where(n => n != null));

			this.FileCommentLines.Clear();
			this.FileCommentLines.AddRange(
				lines
					.Select(n => n.Value as FileCommentLine)
					.Where(n => n != null));

			this.HistoryLines.Clear();
			this.HistoryLines.AddRange(
				lines
					.Select(n => n.Value as HistoryLine)
					.Where(n => n != null));

			this.LicenceLines.Clear();
			this.LicenceLines.AddRange(
				lines
					.Select(n => n.Value as LicenceLine)
					.Where(n => n != null));

			this.OpCodeLines.Clear();
			this.OpCodeLines.AddRange(
				lines
					.Select(n => n.Value as OpCodeLine)
					.Where(n => n != null));

			this.PropertyLines.Clear();
			this.PropertyLines.AddRange(
				lines
					.Select(n => n.Value as PropertyLine)
					.Where(n => n != null));

			this.EnumerationNames.Clear();
			this.EnumerationNames.AddRange(
				this.EnumerationLines
					.Select(n => n.EnumName)
					.Distinct());

			this.OpCodeNumbers.Clear();
			this.OpCodeNumbers.AddRange(
				this.OpCodeLines
                    .Where(n => n is not OpCodeReservedLine)
					.Select(n => n.Value)
					.Distinct());
		}
	}

	public class Builder
    {
        private readonly Loader loader;

		public FileCommentBlock FileCommentBlock { get; private set; }
		public HistoryBlock HistoryBlock { get; private set; }
		public LicenceBlock LicenceBlock { get; private set; }
		public List<OpCodeBlock> OpCodeBlocks { get; } = new();

        public List<int> OpCodeBaseAbstractClassSuffixes { get; } = new();

        public Builder(Loader loader)
        {
            this.loader = loader;
        }

		public void Build()
        {
			this.FileCommentBlock = new FileCommentBlock(this.loader.FileCommentLines);
			this.HistoryBlock = new HistoryBlock(this.loader.HistoryLines);
			this.LicenceBlock = new LicenceBlock(this.loader.LicenceLines);

            this.OpCodeBlocks.Clear();
            foreach (var value in this.loader.OpCodeNumbers)
            {
				var opCodeBlock = new OpCodeBlock(value, this.loader.OpCodeLines, this.loader.PropertyLines);
				this.OpCodeBlocks.Add(opCodeBlock);
            }

            this.OpCodeBaseAbstractClassSuffixes.Clear();
            this.OpCodeBaseAbstractClassSuffixes.AddRange(
                this.loader.OpCodeNumbers
                    .Select(n => n >> 5)
                    .Distinct());
        }
    }

    #region Builder model

    public class FileCommentBlock
    {
        public string Text { get; }

        public FileCommentBlock(List<FileCommentLine> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
                sb.AppendLine($" *\t{line.Text}");
            this.Text = sb.ToString();
        }
    }

    public class HistoryBlock
    {
        public string Text { get; }

        public HistoryBlock(List<HistoryLine> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
                sb.AppendLine($" *\t{line.Text}");
            this.Text = sb.ToString();
        }
    }

    public class LicenceBlock
    {
        public string Text { get; }

        public LicenceBlock(List<LicenceLine> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
                sb.AppendLine($" *\t{line.Text}");
            this.Text = sb.ToString();
        }
    }

    public class OpCodeBlock
    {
        //# "opcode",Value (hex),"values",Code,Name,Priority,Group
        //# "opcode",Value (hex),"description",Text
        //# "opcode",Value (hex),"property",Source,Name
        //# "opcode",Value (hex),"tostring",Format-string
        //# "opcode",Value (hex),"comment",Text

        public byte Value { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Priority { get; private set; }
        public string Group { get; private set; }

        public string Description { get; private set; }

        public string ToStringText { get; private set; }

        public List<string> Comments { get; } = new();

        public List<OpCodeProperty> Properties { get; } = new();

        public string BaseClassName => $"OpCodeData{this.Value >> 5}";

        public OpCodeBlock(byte value, List<OpCodeLine> opCodeLines, List<PropertyLine> propertyLines)
        {
            this.Value = value;

            var opCodeValueLine =
                opCodeLines
                    .Where(n => n.Value == value)
                    .Select(n => n as OpCodeValueLine)
                    .Where(n => n != null)
                    .FirstOrDefault();

            this.Code = opCodeValueLine.Code;
            this.Name = opCodeValueLine.Name;
            this.Priority = opCodeValueLine.Priority;
            this.Group = opCodeValueLine.Group;

            var sb = new StringBuilder();
            opCodeLines
                .Where(n => n.Value == value)
                .Select(n => n as OpCodeDescriptionLine)
                .Where(n => n != null)
                .ToList()
                .ForEach(n => sb.AppendLine(n.Text));
            this.Description = sb.ToString().Trim(); ;

            this.ToStringText =
                opCodeLines
                    .Where(n => n.Value == value)
                    .Select(n => n as OpCodeToStringLine)
                    .Where(n => n != null)
                    .FirstOrDefault()?.Text;

            this.Comments.AddRange(
                opCodeLines
                    .Where(n => n.Value == value)
                    .Select(n => n as OpCodeCommentLine)
                    .Where(n => n != null)
                    .Select(n => n.Text));

            this.Properties =
                opCodeLines
                    .Where(n => n.Value == value)
                    .Select(n => n as OpCodePropertyLine)
                    .Where(n => n != null)
                    .Select(n => new OpCodeProperty(n, propertyLines))
                    .ToList();

            if (string.IsNullOrEmpty(this.ToStringText))
            {
                sb.Clear();
                sb.Append("{" + "this.Number" + "}");
                foreach (var property in this.Properties.OrderBy(p => p.Source))
                {
                    var format = GetFormat(property);
                    var variable = GetValue(property);
                    sb.Append(" {" + variable + format + "}");
                }
                this.ToStringText = "\"" + sb.ToString() + "\"";
            }
        }

        private static string GetFormat(OpCodeProperty property)
        {
            switch (property.Format)
            {
                case "char":
                    return string.Empty;
                case "decimal":
                    return string.Empty;
                case "Enum":
                    return ":F";
                case "hex":
                    return ":X2";
                default:
                    return string.Empty;
            };
        }

        private static string GetValue(OpCodeProperty property)
        {
            switch (property.Format)
            {
                case "char":
                case "decimal":
                case "Enum":
                case "hex":
                    return "this." + property.Name;
                default:
                    if (property.Format.Contains('|'))
                    {
                        var items = property.Format.Split('|').Take(2).ToArray();
                        return "(this." + property.Name + " ? \"" + items[0] + "\" : \"" + items[1] + "\")";
                    }
                    return "this." + property.Name;
            }
        }
    }

    public class OpCodeProperty
    {
        //# "opcode",Value (hex),"property",Source,Name
        // # "property",Name,DataType

        public byte OpCodeValue { get; }
        public string Source { get; }
        public char[] ByteIndexes { get; }
        public char[] BitIndexes { get; }
        public string Name { get; }
        public string DataType { get; }
        public string Format { get; }

        public OpCodeProperty(OpCodePropertyLine opCodePropertyLine, List<PropertyLine> propertyLines)
        {
            this.OpCodeValue = opCodePropertyLine.Value;
            this.Source = opCodePropertyLine.Source;
            this.Name = opCodePropertyLine.Name;

            var propertyLine =
                propertyLines
                    .Where(pl => pl.Name == this.Name)
                    .FirstOrDefault();
            this.DataType = propertyLine?.DataType ?? "byte";
            this.Format = propertyLine?.Format ?? "decimal";

            if (this.Source.Contains(':'))
            {
                var items = this.Source.Split(':');
                this.ByteIndexes = items[0].ToCharArray();
                this.BitIndexes = items[1].ToCharArray();
            }
            else
            {
                this.ByteIndexes = this.Source.ToCharArray();
                this.BitIndexes = System.Array.Empty<char>();
            }
        }
    }

    #endregion

    #region Loader model

    public abstract class Line
    {
        public int Number { get; set; }

        protected string[] Items { get; set; }

        protected Line(int number, string text)
        {
            this.Number = number;

            this.Items = text.Split(',');
        }

        public static Line Create(int number, string text)
        {
            if (text.Trim().StartsWith("#") ||
                text.Trim().StartsWith(";") ||
                text.Trim().StartsWith("!") ||
                text.Trim().StartsWith("//"))
            {
                return CommentLine.Create(number, text);
            }

            if (string.IsNullOrEmpty(text))
                return BlankLine.Create(number);

            if (text.Trim().StartsWith("comment,"))
                return FileCommentLine.Create(number, text);

            if (text.Trim().StartsWith("enumeration,"))
                return EnumerationLine.Create(number, text);

            if (text.Trim().StartsWith("History,"))
                return HistoryLine.Create(number, text);

            if (text.Trim().StartsWith("Licence,"))
                return LicenceLine.Create(number, text);

            if (text.Trim().StartsWith("opcode,"))
                return OpCodeLine.Create(number, text);

            if (text.Trim().StartsWith("property,"))
                return PropertyLine.Create(number, text);

            return null;
        }
    }

    public class BlankLine : Line
    {
        private BlankLine(int number) : base(number, string.Empty) { }

        public static BlankLine Create(int number)
        {
            var result = new BlankLine(number);
            return result;
        }
    }

    public class CommentLine : Line
    {
        public string Text { get; }

        private CommentLine(int number, string text)
            : base(number, text)
        {
            this.Text = text;
        }

        public new static CommentLine Create(int number, string text)
        {
            var result = new CommentLine(number, text);
            return result;
        }
    }

    public class EnumerationLine : Line
    {
        public string EnumName { get; set; }
        public int Value { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public bool IsFlags { get; }

        private EnumerationLine(int number, string text)
            : base(number, text)
        {
            //   0             1        2     3        4
            // # "enumeration",EnumName,Value,ItemName,Description

            if (this.Items.Length <= 4) return;

            this.EnumName = this.Items[1];
            var value = this.Items[2].ToUpper();
            if (!value.StartsWith("0X"))
            {
                this.IsFlags = true;
            }

            this.Value = this.IsFlags
                ? int.Parse(value)
                : int.Parse(value.Replace("0X", string.Empty),
                            NumberStyles.HexNumber);

            this.ItemName = this.Items[3];
            this.Description = this.Items[4];
        }

        public static new EnumerationLine Create(int number, string text)
        {
            var result = new EnumerationLine(number, text);
            if (result.Items.Length <= 4)
                return null;
            if (string.IsNullOrEmpty(result.EnumName))
                return null;
            if (string.IsNullOrEmpty(result.ItemName))
                return null;
            return result;
        }

        public override string ToString() =>
            $"{this.EnumName} : {this.Value} : {this.ItemName} : {this.Description}";
    }

    public class FileCommentLine : Line
    {
        public string Text { get; }

        private FileCommentLine(int number, string text)
            : base(number, text)
        {
            // # "Comment",text

            this.Text = string.Join(",", base.Items.Skip(1));
        }

        public new static FileCommentLine Create(int number, string text)
        {
            var result = new FileCommentLine(number, text);
            return result;
        }
    }

    public class HistoryLine : Line
    {
        public string Text { get; }

        private HistoryLine(int number, string text)
            : base(number, text)
        {
            // # "History",date(y-m-d),author,text

            this.Text = string.Join("\t", base.Items.Skip(1));
        }

        public new static HistoryLine Create(int number, string text)
        {
            var result = new HistoryLine(number, text);
            return result;
        }

        public override string ToString() => this.Text;
    }

    public class LicenceLine : Line
    {
        public string Text { get; }

        private LicenceLine(int number, string text)
            : base(number, text)
        {
            //   0         1
            // # "Licence",text

            this.Text = string.Join(",", base.Items.Skip(1));
        }

        public new static LicenceLine Create(int number, string text)
        {
            var result = new LicenceLine(number, text);
            return result;
        }

        public override string ToString() => this.Text;
    }

    public abstract class OpCodeLine : Line
    {
        protected abstract string TypeName { get; set; }

        public byte Value { get; }

        protected OpCodeLine(int number, string text)
            : base(number, text)
        {
            //   0        1           
            // # "opcode",Value (hex),

            var value = this.Items[1].ToUpper().Replace("0X", string.Empty);
            this.Value = byte.Parse(value, NumberStyles.HexNumber);
            this.Items = this.Items.ToArray();
        }

        public static new OpCodeLine Create(int number, string text)
        {
            var items = text.Split(',');

            if (items.Length < 3) return null;

            if (items[2].Trim() == "values")
                return OpCodeValueLine.Create(number, text);
            if (items[2].Trim() == "reserved")
                return OpCodeReservedLine.Create(number, text);
            if (items[2].Trim() == "description")
                return OpCodeDescriptionLine.Create(number, text);
            if (items[2].Trim() == "property")
                return OpCodePropertyLine.Create(number, text);
            if (items[2].Trim() == "tostring")
                return OpCodeToStringLine.Create(number, text);

            return null;
        }

        public override string ToString() => $"0x{this.Value:X2} {this.TypeName}: ";
    }

    public class OpCodeCommentLine : OpCodeLine
    {
        protected override string TypeName { get; set; } = "Comment";

        public string Text { get; }

        private OpCodeCommentLine(int number, string text)
            : base(number, text)
        {
            //   0        1           2         3
            // # "opcode",Value (hex),"comment",Text

            if (this.Items.Length < 4) return;

            this.Text = this.Items[3];
        }

        public new static OpCodeCommentLine Create(int number, string text)
        {
            var result = new OpCodeCommentLine(number, text);
            if (result.Items.Length < 4)
                return null;
            return result;
        }

        public override string ToString() => $"{base.ToString()}{this.Text}";
    }

    public class OpCodeDescriptionLine : OpCodeLine
    {
        protected override string TypeName { get; set; } = "Description";

        public string Text { get; }

        private OpCodeDescriptionLine(int number, string text)
            : base(number, text)
        {
            //   0        1           2             3
            // # "opcode",Value (hex),"description",Text

            if (this.Items.Length < 4) return;

            this.Text = this.Items[3].Trim('"');
        }

        public new static OpCodeDescriptionLine Create(int number, string text)
        {
            var result = new OpCodeDescriptionLine(number, text);
            if (result.Items.Length < 4)
                return null;
            return result;
        }

        public override string ToString() => $"{base.ToString()}{this.Text}";
    }

    public class OpCodePropertyLine : OpCodeLine
    {
        protected override string TypeName { get; set; } = "Property";

        public string Name { get; }
        public string Source { get; }

        private OpCodePropertyLine(int number, string text)
            : base(number, text)
        {
            //   0        1           2          3      4
            // # "opcode",Value (hex),"property",Source,Name

            this.Source = this.Items[3].Trim('"');
            this.Name = this.Items[4].Trim('"');
        }

        public new static OpCodePropertyLine Create(int number, string text)
        {
            var result = new OpCodePropertyLine(number, text);
            if (result.Items.Length < 5)
                return null;
            return result;
        }

        public override string ToString() => $"{base.ToString()}{this.Source} : {this.Name}";
    }

    public class OpCodeReservedLine : OpCodeLine
    {
        protected override string TypeName { get; set; } = "Reserved";

        public string Reason { get; }

        private OpCodeReservedLine(int number, string text)
            : base(number, text)
        {
            //   0        1           2
            // # "opcode",Value (hex),"reserved"

            if (this.Items.Length < 3) return;

            this.Reason = this.Items[2];
        }

        public new static OpCodeReservedLine Create(int number, string text)
        {
            var result = new OpCodeReservedLine(number, text);
            if (result.Items.Length < 3)
                return null;
            return result;
        }

        public override string ToString() => $"{base.ToString()}{this.Reason}";
    }

    public class OpCodeToStringLine : OpCodeLine
    {
        protected override string TypeName { get; set; } = "ToString";

        public string Text { get; }

        private OpCodeToStringLine(int number, string text)
            : base(number, text)
        {
            //   0        1           2          3
            // # "opcode",Value (hex),"tostring",Format-string

            if (this.Items.Length < 4) return;

            this.Text = this.Items[3];
        }

        public new static OpCodeToStringLine Create(int number, string text)
        {
            var result = new OpCodeToStringLine(number, text);
            if (result.Items.Length < 4)
                return null;
            return result;
        }

        public override string ToString() => $"{base.ToString()}{this.Text}";
    }

    public class OpCodeValueLine : OpCodeLine
    {
        protected override string TypeName { get; set; } = "Value";

        public string Code { get; }
        public string Name { get; }
        public string Priority { get; }
        public string Group { get; }

        private OpCodeValueLine(int number, string text)
            : base(number, text)
        {
            //   0        1           2        3    4    5        6
            // # "opcode",Value (hex),"values",Code,Name,Priority,Group

            if (this.Items.Length < 7) return;

            this.Code = this.Items[3].Trim('"');
            this.Name = this.Items[4].Trim('"');
            this.Priority = this.Items[5];
            this.Group = this.Items[6];
        }

        public new static OpCodeValueLine Create(int number, string text)
        {
            var result = new OpCodeValueLine(number, text);
            if (result.Items.Length < 7)
                return null;
            return result;
        }

        public override string ToString() =>
            $"{base.ToString()}{this.Code} : {this.Name} : {this.Priority} : {this.Group}";
    }

    public class PropertyLine : Line
    {
        public string Name { get; }
        public string DataType { get; }
        public string Format { get; }

        private PropertyLine(int number, string text)
            : base(number, text)
        {
            //   0          1    2        3
            // # "property",Name,DataType,Format

            if (this.Items.Length < 4) return;

            this.Name = this.Items[1];
            this.DataType = this.Items[2];
            this.Format = this.Items[3];
        }

        public new static PropertyLine Create(int number, string text)
        {
            var result = new PropertyLine(number, text);
            if (result.Items.Length < 3)
                return null;
            return result;
        }

        public override string ToString() => $"{this.Name} : {this.DataType}";
    }

    #endregion
}

#pragma warning restore IDE0021 // Use expression body for constructors
#pragma warning restore IDE0066 // Convert switch statement to expression
