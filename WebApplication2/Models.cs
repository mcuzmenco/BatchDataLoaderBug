using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Resolvers;

namespace WebApplication2
{
	public class Query
	{
		int requestCount = 0;

		public async Task<Book> Book(int id)
		{
			await Task.Delay(50);
			return new Book { Title = "C# in depth", Author = "Jon Skeet", Id = id };
		}

		public async Task<List<Book>> GetBooks(IResolverContext context)
		{
			requestCount = 0;
			var ids = Enumerable.Range(0, 100).ToImmutableList();

			await Task.Delay(100);

			var dataLoader = context.BatchDataLoader<int, Book>(async (bookIds, t) => await LoadBooks(bookIds));
			var books = await dataLoader.LoadAsync(ids, context.RequestAborted);
			return books.ToList();
		}

		Task<IReadOnlyDictionary<int, Book>> LoadBooks(IReadOnlyList<int> bookIds)
		{
			Interlocked.Increment(ref requestCount);
			if (requestCount > 1)
			{
				throw new Exception("Nope!");
			}

			var data = bookIds.Select(
				x => new Book
				{
					Title = "C# in depth",
					Author = "Jon Skeet",
					Id = x
				}
			);

			IReadOnlyDictionary<int, Book> result = data.ToDictionary(x => x.Id);
			return Task.FromResult(result);
		}
	}


	public class Book
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Author { get; set; }
	}
}
