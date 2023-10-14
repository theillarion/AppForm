using System;

namespace Xk7.Model
{
	internal class DbTest : IDbTest, IComparable
    {
		public int Id { get; set; } = 0;
		public string Tittle { get; set; } = string.Empty;
		public string? Description { get; set; } = null;
		public DateTime? _utcDateTimeCreated = null;
		public DateTime UtcDateTimeCreated
		{
			get
			{
				if (_utcDateTimeCreated == null)
					_utcDateTimeCreated = DateTime.UtcNow;
                return _utcDateTimeCreated ?? DateTime.UtcNow;
			}
			set
			{
				_utcDateTimeCreated = value;
            }
		}
		public string WhoCreated { get; set; } = string.Empty;
		public bool HasImage { get; set; } = false;
		public DbTest() { }
        public DbTest(int id, string tittle, string? description, DateTime utcDateTimeCreated, string whoCreated, bool hasImage)
		{
			Id = id;
			Tittle = tittle;
			Description = description;
			UtcDateTimeCreated = utcDateTimeCreated;
			WhoCreated = whoCreated;
			HasImage = hasImage;
		}
		public bool AllFieldsFilled() => Tittle != string.Empty && _utcDateTimeCreated != null && WhoCreated != string.Empty;
        public int CompareTo(object? other)
        {
			if (other != null && other is DbTest dbtest)
				return Id.CompareTo(dbtest.Id);
			return 1;
        }
    }
	internal class DbTestWithImage : DbTest, IDbTestWithImage
    {
		public string Path { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
		public DbTestWithImage() { }
        public DbTestWithImage(int id, string tittle, string? description, DateTime utcDateTimeCreated, string whoCreated, bool hasImage, string pathImage, string localPathImage)
			: base(id, tittle, description, utcDateTimeCreated, whoCreated, hasImage)
		{
			Path = pathImage;
			LocalPath = localPathImage;
		}
		new public bool AllFieldsFilled() => base.AllFieldsFilled() && Path != string.Empty;
    }
}