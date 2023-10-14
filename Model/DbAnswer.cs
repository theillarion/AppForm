using Org.BouncyCastle.Crypto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Model
{
	internal class DbSimpleAnswer : IDbAnswer, IComparable
	{
		public int Id { get; set; } = 0;
		public int IdQuestion { get; set; } = 0;
		private string? _text = null;
		public string Text
		{
			get => _text ?? string.Empty;
			set
			{
				_text = value;
			}
		}
		public bool IsCorrect { get; set; } = false;
		public DbSimpleAnswer() { }
		public DbSimpleAnswer(int id, int idQuestion, string text, bool isCorrect)
		{
			Id = id;
			IdQuestion = idQuestion;
			Text = text;
			IsCorrect = isCorrect;
		}
		public bool AllFieldsFilled() => IdQuestion != 0 && _text != null;
		public int CompareTo(object? other)
		{
			if (other != null && other is DbSimpleAnswer dbSimpleAnswer)
			{
                int result = IdQuestion.CompareTo(dbSimpleAnswer.IdQuestion);
                return (result != 0) ? result : Id.CompareTo(dbSimpleAnswer.Id);
            }
			return 1;
		}
	}
}