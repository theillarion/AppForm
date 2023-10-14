using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Xk7.Model.DbQuestion;

namespace Xk7.Model
{
	internal class DbQuestion : IDbQuestion, IComparable
	{
		public int Id { get; set; } = 0;
		public int IdTest { get; set; } = 0;
		public uint IdType { get; set; } = 1;
		public int OrdinalNumber { get; set; } = 0;
		public string Text { get; set; } = string.Empty;
		public bool HasImage { get; set; } = false;
		public DbQuestion() { }
		public DbQuestion(int id, int idTest, uint idType, int ordinalNumber, string text, bool hasImage)
		{
			Id = id;
			IdTest = idTest;
			IdType = idType;
			OrdinalNumber = ordinalNumber;
			Text = text;
			HasImage = hasImage;
		}
		public DbQuestion(int idTest, string text)
		{
			IdTest = idTest;
			Text = text;
		}
		public bool AllFieldsFilled() => IdTest > 0 & Text != string.Empty;
		public int CompareTo(object? other)
		{
			if (other != null && other is DbQuestion dbQuestion)
			{
				int result = IdTest.CompareTo(dbQuestion.IdTest);
				int ordinalNumber = OrdinalNumber.CompareTo(dbQuestion.OrdinalNumber);
				return (result != 0) ? result : (ordinalNumber != 0) ? ordinalNumber : Id.CompareTo(dbQuestion.Id);
			}
			return 1;	
		}
	}
	internal class DbQuestionWithImage : DbQuestion, IDbQuestionWithImage
	{
		public string Path { get; set; } = string.Empty;
		public string LocalPath { get; set; } = string.Empty;
		public DbQuestionWithImage() { }
		public DbQuestionWithImage(int id, int idTest, uint idType, int ordinalNumber, string text, bool hasImage, string path, string localPath) : base(id, idTest, idType, ordinalNumber, text, hasImage)
		{
			Path = path;
			LocalPath = localPath;
		}
		new public bool AllFieldsFilled() => base.AllFieldsFilled() && Path != string.Empty;
	}
}